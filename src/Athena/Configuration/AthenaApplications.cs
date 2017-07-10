using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Athena.Logging;
using Athena.PubSub;

namespace Athena.Configuration
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    
    public sealed class AthenaApplications : AthenaSetupContext, AthenaBootstrapper
    {
        private readonly Stopwatch _timer;
        
        private readonly ConcurrentDictionary<string, PartConfiguration> _partConfigurations =
            new ConcurrentDictionary<string, PartConfiguration>();
        
        private readonly ConcurrentDictionary<string, AppFunctionBuilder> _applicationBuilders
            = new ConcurrentDictionary<string, AppFunctionBuilder>();

        private AthenaApplications(string applicationName, string environment,
            IReadOnlyCollection<Assembly> applicationAssemblies, Stopwatch timer)
        {
            Environment = environment;
            ApplicationAssemblies = applicationAssemblies;
            _timer = timer;
            ApplicationName = applicationName;
            SetupEnvironment = new Dictionary<string, object>();
        }
        
        public string ApplicationName { get; }
        public string Environment { get; }
        public IDictionary<string, object> SetupEnvironment { get; }
        public IReadOnlyCollection<Assembly> ApplicationAssemblies { get; }

        public PartConfiguration<TPart> Part<TPart>(string key = null) where TPart : class, new()
        {
            if (string.IsNullOrEmpty(key))
                key = typeof(TPart).AssemblyQualifiedName;

            return _partConfigurations.GetOrAdd(key, x =>
            {
                return new PartConfiguration<TPart>(this, key, (childKey, child) =>
                {
                    _partConfigurations[childKey] = child;
                });
            }) as PartConfiguration<TPart>;
        }

        public Task DefineApplication(string name, Func<AppFunctionBuilder, AppFunctionBuilder> builder)
        {
            Logger.Write(LogLevel.Debug, $"Defining application with name {name}");

            _applicationBuilders.AddOrUpdate(name, _ =>
                {
                    Logger.Write(LogLevel.Debug, $"Defining application with name {name}");
                    
                    return builder(new AppFunctionBuilder(this));
                }, 
                (_, __) =>
                {
                    Logger.Write(LogLevel.Info, $"Application \"{name}\" already exists, redifining");
                    
                    return builder(new AppFunctionBuilder(this));
                });

            return Done(new ApplicationDefined(name));
        }

        public Task UpdateApplication(string name, Func<AppFunctionBuilder, AppFunctionBuilder> builder)
        {
            _applicationBuilders.AddOrUpdate(name, 
                _ => throw new InvalidOperationException($"There is no application named {name}"), 
                (_, current) =>
                {
                    Logger.Write(LogLevel.Debug, $"Updating application \"{name}\"");
                    
                    return builder(current);
                });
            
            return Done(new ApplicationDefinitionModified(name));
        }

        public async Task<AthenaContext> Build()
        {
            Logger.Write(LogLevel.Debug, "Starting context build");
            
            await Done(new BootstrapStarted(ApplicationName, Environment)).ConfigureAwait(false);

            await Done(new BeforeApplicationsCompilation()).ConfigureAwait(false);

            var compilationResults = await Task.WhenAll(_applicationBuilders
                    .Select(x => CompileApplication(x.Key, x.Value)))
                .ConfigureAwait(false);

            var applications = compilationResults.ToDictionary(x => x.Item1, x => x.Item2);

            await Done(new ApplicationsCompiled()).ConfigureAwait(false);

            var context = await ApplicationsContext.From(ApplicationAssemblies, ApplicationName, Environment,
                    applications,
                    _partConfigurations)
                .ConfigureAwait(false);

            await Done(new BootstrapCompleted(ApplicationName, Environment, _timer.Elapsed)).ConfigureAwait(false);
            
            Logger.Write(LogLevel.Debug, "Context build finished");

            return context;
        }
        
        private async Task Done(SetupEvent evnt)
        {
            Logger.Write(LogLevel.Debug, $"{evnt} done");
            
            await Task.WhenAll(_partConfigurations
                    .Select(x => x.Value.RunSetupsFor(evnt, this)))
                .ConfigureAwait(false);

            EventPublishing.Publish(evnt, SetupEnvironment);
        }

        private async Task<Tuple<string, AppFunc>> CompileApplication(string name, AppFunctionBuilder builder)
        {
            var timer = Stopwatch.StartNew();
            
            var result = builder.Compile();
            
            timer.Stop();

            await Done(new ApplicationCompiled(name, result.Item2, timer.Elapsed));
            
            return new Tuple<string, AppFunc>(name, result.Item1);
        }
        
        public static AthenaBootstrapper From(string environment, params Assembly[] applicationAssemblies)
        {
            return From(environment, Assembly.GetEntryAssembly().GetName().Name.Replace(".", ""),
                applicationAssemblies);
        }

        public static AthenaBootstrapper From(string environment, string applicationName,
            params Assembly[] applicationAssemblies)
        {
            var timer = Stopwatch.StartNew();
            
            AthenaBootstrapper bootstrapper = new AthenaApplications(applicationName, environment, 
                applicationAssemblies, timer);

            var componentType = typeof(AthenaComponent);

            var components = GetAllAssemblies(Assembly.GetEntryAssembly())
                .SelectMany(x => x.GetTypes())
                .Where(x =>
                {
                    var typeInfo = x.GetTypeInfo();

                    return componentType.GetTypeInfo().IsAssignableFrom(x)
                           && !typeInfo.IsAbstract
                           && !typeInfo.IsInterface
                           && x.GetTypeInfo().GetConstructors().Any(y => !y.GetParameters().Any() && y.IsPublic);
                })
                .Select(Activator.CreateInstance)
                .OfType<AthenaComponent>()
                .ToList();

            return components.Aggregate(bootstrapper, (current, component) => component.Configure(current));
        }
        
        private static IEnumerable<Assembly> GetAllAssemblies(Assembly from)
        {
            var assemblies = new List<Assembly>
            {
                from
            };

            foreach (var referencedAssembly in from.GetReferencedAssemblies())
            {
                var assembly = Assembly.Load(referencedAssembly);

                if (assemblies.All(x => x.FullName != assembly.FullName))
                    assemblies.Add(assembly);
            }

            return assemblies;
        }
    }
}