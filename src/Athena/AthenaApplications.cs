using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Athena.Messages;
using Athena.PubSub;
using Microsoft.Extensions.DependencyModel;

namespace Athena
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    
    public sealed class AthenaApplications : AthenaContext, AthenaBootstrapper
    {
        private readonly IDictionary<string, AppFunctionBuilder> _applicationBuilders
            = new ConcurrentDictionary<string, AppFunctionBuilder>();

        private IReadOnlyDictionary<string, AppFunc> _applications;
        
        private readonly IEnumerable<AthenaPlugin> _plugins;

        public static IReadOnlyCollection<Assembly> ApplicationAssemblies { get; private set; }

        private AthenaApplications(IEnumerable<AthenaPlugin> plugins)
        {
            _plugins = plugins;
        }

        public string ApplicationName { get; private set; } = Assembly.GetEntryAssembly().GetName().Name.Replace(".", "");

        public AthenaBootstrapper DefineApplication(string name, Func<AppFunctionBuilder, AppFunctionBuilder> builder, 
            bool overwrite = true)
        {
            if (overwrite || !_applicationBuilders.ContainsKey(name))
                _applicationBuilders[name] = builder(new AppFunctionBuilder());

            return this;
        }

        public AthenaBootstrapper ConfigureApplication(string name, Func<AppFunctionBuilder, AppFunctionBuilder> app)
        {
            if (!_applicationBuilders.ContainsKey(name))
                return DefineApplication(name, app);

            _applicationBuilders[name] = app(_applicationBuilders[name]);

            return this;
        }

        public AthenaBootstrapper WithApplicationName(string name)
        {
            ApplicationName = name;

            return this;
        }

        public IReadOnlyCollection<string> GetDefinedApplications()
        {
            return new ReadOnlyCollection<string>(_applicationBuilders.Keys.ToList());
        }

        public async Task Execute(string application, IDictionary<string, object> environment)
        {
            using (environment.EnterApplication(this, application))
                await _applications[application](environment).ConfigureAwait(false);
        }

        public Task ShutDown()
        {
            return Task.WhenAll(_plugins.Select(x => x.TearDown(this)));
        }

        private void Start()
        {
            _applications = _applicationBuilders.ToDictionary(x => x.Key, x => x.Value.Compile());
        }

        public static async Task<AthenaContext> Bootsrap(Action<AthenaBootstrapper> bootstrap,
            params Assembly[] applicationAssemblies)
        {
            var timer = Stopwatch.StartNew();
            
            ApplicationAssemblies = applicationAssemblies;

            var pluginType = typeof(AthenaPlugin);

            var plugins = GetReferencingAssemblies(pluginType.GetTypeInfo().Assembly.GetName().Name)
                .SelectMany(x => x.GetTypes()
                    .Where(y =>
                    {
                        var typeInfo = y.GetTypeInfo();

                        return pluginType.IsAssignableFrom(y) && !typeInfo.IsAbstract && !typeInfo.IsInterface;
                    }))
                .Select(Activator.CreateInstance)
                .OfType<AthenaPlugin>()
                .ToList();
            
            var context = new AthenaApplications(plugins);

            await Task.WhenAll(plugins.Select(x => x.Bootstrap(context))).ConfigureAwait(false);

            bootstrap(context);
            
            context.Start();
            
            timer.Stop();

            await EventPublishing.Publish(new BootstrapCompleted(timer.Elapsed)).ConfigureAwait(false);

            return context;
        }
        
        public static Task<AthenaContext> Bootsrap(params Assembly[] applicationAssemblies)
        {
            return Bootsrap(x => { }, applicationAssemblies);
        }

        private static IEnumerable<Assembly> GetReferencingAssemblies(string assemblyName)
        {
            var dependencies = DependencyContext.Default.RuntimeLibraries;

            return (from library in dependencies
                where IsCandidateLibrary(library, assemblyName)
                select Assembly.Load(new AssemblyName(library.Name))).ToList();
        }

        private static bool IsCandidateLibrary(Library library, string assemblyName)
        {
            return library.Name == assemblyName
                   || library.Dependencies.Any(d => d.Name.StartsWith(assemblyName));
        }
    }
}