using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Athena.Configuration
{
    public sealed class AthenaApplications : AthenaBootstrapper, AthenaSetupContext
    {
        private readonly ConcurrentBag<AthenaPlugin> _plugins = new ConcurrentBag<AthenaPlugin>();
        private readonly Stopwatch _timer;
        
        private readonly IDictionary<string, AppFunctionBuilder> _applicationBuilders
            = new ConcurrentDictionary<string, AppFunctionBuilder>();
        
        public string ApplicationName { get; private set; } =
            Assembly.GetEntryAssembly().GetName().Name.Replace(".", "");

        public string Environment { get; }
        public IReadOnlyCollection<Assembly> ApplicationAssemblies { get; }

        private readonly ConcurrentBag<Tuple<Type, Func<object, AthenaSetupContext, Task>>> _eventListeners 
            = new ConcurrentBag<Tuple<Type, Func<object, AthenaSetupContext, Task>>>();

        private AthenaApplications(string environment, IReadOnlyCollection<Assembly> applicationAssemblies)
        {
            Environment = environment;
            ApplicationAssemblies = applicationAssemblies;
            _timer = Stopwatch.StartNew();
        }

        public void DefineApplication(string name, Func<AppFunctionBuilder, AppFunctionBuilder> builder)
        {
            if(_applicationBuilders.ContainsKey(name))
                throw new InvalidOperationException($"There is already a application named {name}");
            
            _applicationBuilders[name] = builder(new AppFunctionBuilder());
        }

        public void ConfigureApplication(string name, 
            Func<AppFunctionBuilder, AppFunctionBuilder> builder)
        {
            if(!_applicationBuilders.ContainsKey(name))
                throw new InvalidOperationException($"There is no application named {name}");
            
            _applicationBuilders[name] = builder(_applicationBuilders[name]);
        }

        public AthenaBootstrapper WithApplicationName(string name)
        {
            ApplicationName = name;

            return this;
        }

        public PartConfiguration<TPlugin> UsingPlugin<TPlugin>(TPlugin plugin) where TPlugin : class, AthenaPlugin
        {
            var existing = _plugins.FirstOrDefault(x => x.GetType() == plugin.GetType()) as TPlugin;

            if (existing != null)
                return new PartConfiguration<TPlugin>(this, existing);
            
            _plugins.Add(plugin);

            return new PartConfiguration<TPlugin>(this, plugin);
        }

        public IReadOnlyCollection<string> GetDefinedApplications()
        {
            return _applicationBuilders.Keys.ToList();
        }

        public async Task Done(SetupEvent evnt)
        {
            foreach (var type in evnt.GetType().GetParentTypesFor())
            {
                var subscriptions = _eventListeners.Where(x => x.Item1 == type).ToList();

                await Task.WhenAll(subscriptions.Select(x => x.Item2(evnt, this))).ConfigureAwait(false);
            }
        }

        public BootstrapEventListenerSetup<TEvent> When<TEvent>() where TEvent : SetupEvent
        {
            return new BootstrapEventListenerSetup<TEvent>(listener => 
                _eventListeners.Add(new Tuple<Type, Func<object, AthenaSetupContext, Task>>(typeof(TEvent), 
                    (evnt, context) => listener((TEvent)evnt, context))), this);
        }

        public async Task<AthenaContext> Build()
        {
            var timer = Stopwatch.StartNew();
            
            await Task.WhenAll(_plugins.Select(BootstrapPlugin)).ConfigureAwait(false);
            
            timer.Stop();

            await Done(new AllPluginsBootstrapped(timer.Elapsed)).ConfigureAwait(false);

            var applications = _applicationBuilders.ToDictionary(x => x.Key, x => x.Value.Compile());
            
            await Done(new BootstrapCompleted(ApplicationName, Environment, _timer.Elapsed)).ConfigureAwait(false);

            await Done(new ApplicationsStarted(ApplicationName, Environment, _timer.Elapsed)).ConfigureAwait(false);
            
            var context = new ApplicationsContext(ApplicationAssemblies, ApplicationName, Environment, applications, _plugins);

            await Done(new ContextCreated(_timer.Elapsed, context)).ConfigureAwait(false);

            return context;
        }

        public static AthenaBootstrapper From(string environment, params Assembly[] applicationAssemblies)
        {
            return new AthenaApplications(environment, applicationAssemblies);
        }

        private async Task BootstrapPlugin(AthenaPlugin plugin)
        {
            var timer = Stopwatch.StartNew();

            await plugin.Bootstrap(this).ConfigureAwait(false);
            
            timer.Stop();

            await Done(new PluginBootstrapped(timer.Elapsed, plugin.GetType())).ConfigureAwait(false);
        }
    }
}