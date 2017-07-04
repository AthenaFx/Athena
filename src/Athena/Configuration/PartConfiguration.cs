using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Athena.Logging;

namespace Athena.Configuration
{
    public abstract class PartConfiguration : AthenaBootstrapper
    {
        private readonly AthenaBootstrapper _bootstrapper;

        private readonly ICollection<Func<object, object, AthenaSetupContext, Task<object>>> _configureParent = 
            new List<Func<object, object, AthenaSetupContext, Task<object>>>();

        protected readonly ConcurrentDictionary<string, PartConfiguration> Children =
            new ConcurrentDictionary<string, PartConfiguration>();

        protected PartConfiguration(AthenaBootstrapper bootstrapper, string key)
        {
            _bootstrapper = bootstrapper;
            Key = key;
        }

        public string Key { get; }

        public string ApplicationName => _bootstrapper.ApplicationName;
        public string Environment => _bootstrapper.Environment;
        public IReadOnlyCollection<Assembly> ApplicationAssemblies => _bootstrapper.ApplicationAssemblies;

        public PartConfiguration<TPart> Part<TPart>(string key = null) where TPart : class, new()
        {
            return _bootstrapper.Part<TPart>(key);
        }

        public Task<AthenaContext> Build()
        {
            return _bootstrapper.Build();
        }

        internal abstract Task RunSetupsFor(SetupEvent evnt, AthenaSetupContext context);
        
        internal abstract Task Startup(AthenaContext context);
        internal abstract Task Shutdown(AthenaContext context);
        
        protected void AddParentConfigurer(Func<object, object, AthenaSetupContext, Task<object>> config)
        {
            _configureParent.Add(config);
        }

        internal async Task<object> ConfigureParent(object parent, AthenaSetupContext context)
        {
            Logger.Write(LogLevel.Debug, $"Configuring parent {parent?.GetType()}");
            
            var me = GetPart();

            foreach (var child in Children)
                me = await child.Value.ConfigureParent(me, context).ConfigureAwait(false);

            foreach (var configureParent in _configureParent)
                parent = await configureParent(parent, me, context).ConfigureAwait(false);

            return parent;
        }

        internal abstract object GetPart();
    }

    public class ChildPartConfiguration<TParent, TChild> : PartConfiguration<TChild>
        where TParent : class, new()
        where TChild : class, new()
    {
        public ChildPartConfiguration(AthenaBootstrapper bootstrapper, string key, 
            Action<string, PartConfiguration> addPartConfiguration) : base(bootstrapper, key, addPartConfiguration)
        {
        }
        
        public ChildPartConfiguration<TParent, TChild> ConfigureParentWith(
            Func<TParent, TChild, AthenaSetupContext, Task<TParent>> config)
        {
            AddParentConfigurer(async (parent, child, context) => 
                await config((TParent)parent, (TChild)child, context).ConfigureAwait(false));

            return this;
        }
        
        public ChildPartConfiguration<TParent, TChild> ConfigureParentWith(
            Func<TParent, TChild, AthenaSetupContext, TParent> config)
        {
            return ConfigureParentWith((parent, child, context) => Task.FromResult(config(parent, child, context)));
        }
    }
    
    public class PartConfiguration<TPart> : PartConfiguration where TPart : class, new()
    {
        private object _syncRoot = new object();
        
        private bool _hasSetup;
        private bool _running;
        private readonly AthenaBootstrapper _bootstrapper;
        private readonly Action<string, PartConfiguration> _addPartConfiguration;
        private Func<Func<bool, Task>, AthenaContext, Task> _conditionallySubscription;
        
        private readonly
            ConcurrentBag<Tuple<Func<SetupEvent, bool>, Func<object, SetupEvent, AthenaSetupContext, Task>>> _setups =
                new ConcurrentBag<Tuple<Func<SetupEvent, bool>, Func<object, SetupEvent, AthenaSetupContext, Task>>>();

        private readonly ConcurrentBag<Func<AthenaContext, Task>> _startups =
            new ConcurrentBag<Func<AthenaContext, Task>>();
        
        private readonly ConcurrentBag<Func<AthenaContext, Task>> _shutdowns =
            new ConcurrentBag<Func<AthenaContext, Task>>();

        internal PartConfiguration(AthenaBootstrapper bootstrapper, string key, 
            Action<string, PartConfiguration> addPartConfiguration) 
            : base(bootstrapper, key)
        {
            _bootstrapper = bootstrapper;
            _addPartConfiguration = addPartConfiguration;
            Settings = new TPart();
        }
        
        public TPart Settings { get; private set; }

        public PartConfiguration<TPart> Configure(Func<TPart, TPart> configure)
        {
            Logger.Write(LogLevel.Debug, "Configuration started");
            
            Settings = configure(Settings);

            return this;
        }

        public PartConfiguration<TPart> OnSetup(Func<TPart, AthenaSetupContext, Task> setup)
        {
            return On<BootstrapStarted>((part, evnt, context) => setup(part, context));
        }
        
        public PartConfiguration<TPart> OnSetup(Action<TPart, AthenaSetupContext> setup)
        {
            return On<BootstrapStarted>((part, evnt, context) => setup(part, context));
        }

        public PartConfiguration<TPart> ConditionallyRun(Func<Func<bool, Task>, AthenaContext, Task> subscribe)
        {
            _conditionallySubscription = subscribe;

            return this;
        }

        public PartConfiguration<TPart> On<TEvent>(Func<TPart, TEvent, AthenaSetupContext, Task> setup,
            Func<TEvent, bool> filter = null) where TEvent : SetupEvent
        {
            filter = filter ?? (x => true);

            _setups.Add(new Tuple<Func<SetupEvent, bool>, Func<object, SetupEvent, AthenaSetupContext, Task>>(
                x => x is TEvent && filter((TEvent) x),
                (part, evnt, context) => setup((TPart) part, (TEvent) evnt, context)));

            return this;
        }

        public PartConfiguration<TPart> On<TEvent>(Action<TPart, TEvent, AthenaSetupContext> setup,
            Func<TEvent, bool> filter = null) where TEvent : SetupEvent
        {
            return On<TEvent>((part, evnt, context) =>
            {
                setup(part, evnt, context);

                return Task.CompletedTask;
            });
        }
        
        public PartConfiguration<TPart> OnStartup(Func<TPart, AthenaContext, Task> startup)
        {
            _startups.Add(x => startup(Settings, x));

            return this;
        }
        
        public PartConfiguration<TPart> OnStartup(Action<TPart, AthenaContext> startup)
        {
            return OnStartup((part, context) =>
            {
                startup(part, context);

                return Task.CompletedTask;
            });
        }

        public PartConfiguration<TPart> OnShutdown(Func<TPart, AthenaContext, Task> shutdown)
        {
            _shutdowns.Add(x => shutdown(Settings, x));

            return this;
        }
        
        public PartConfiguration<TPart> OnShutdown(Action<TPart, AthenaContext> shutdown)
        {
            return OnShutdown((part, context) =>
            {
                shutdown(part, context);

                return Task.CompletedTask;
            });
        }

        public ChildPartConfiguration<TPart, TChild> Child<TChild>(string key = null) where TChild : class, new()
        {
            Logger.Write(LogLevel.Debug, $"Configuring child ({typeof(TChild)}) for {typeof(TPart)}");
            
            if (string.IsNullOrEmpty(key))
                key = typeof(TPart).AssemblyQualifiedName;

            return Children.GetOrAdd(key, x =>
            {
                var child = new ChildPartConfiguration<TPart, TChild>(_bootstrapper, x, _addPartConfiguration);

                _addPartConfiguration(x, child);

                return child;
            }) as ChildPartConfiguration<TPart, TChild>;
        }
        
        internal override async Task RunSetupsFor(SetupEvent evnt, AthenaSetupContext context)
        {
            var availableSetups = _setups
                .Where(x => x.Item1(evnt))
                .ToList();
            
            if(!availableSetups.Any())
                return;

            Logger.Write(LogLevel.Debug, $"Setting up {typeof(TPart)} for {evnt} ({availableSetups.Count} setups)");
            
            if (!_hasSetup)
            {
                foreach (var child in Children)
                    Settings = (TPart) (await child.Value.ConfigureParent(Settings, context).ConfigureAwait(false));
            }
            
            _hasSetup = true;

            var part = GetPart();
            
            await Task.WhenAll(availableSetups
                .Select(x => x.Item2(part, evnt, context)))
                .ConfigureAwait(false);
        }

        internal override async Task Startup(AthenaContext context)
        {
            if (_conditionallySubscription != null)
            {
                await _conditionallySubscription(shouldRun => ChangeRunningStatus(shouldRun, context), context)
                    .ConfigureAwait(false);
                
                return;
            }
            
            await RunStartups(context).ConfigureAwait(false);
        }

        internal override Task Shutdown(AthenaContext context)
        {
            lock (_syncRoot)
            {
                if (!_running)
                    return Task.CompletedTask;

                _running = false;

                return Task.WhenAll(_shutdowns.Select(y => y(context)));
            }
        }

        internal override object GetPart()
        {
            return Settings;
        }

        private async Task ChangeRunningStatus(bool shouldRun, AthenaContext context)
        {
            if (_running == shouldRun)
                return;

            if (shouldRun)
                await RunStartups(context).ConfigureAwait(false);
            else
                await Shutdown(context).ConfigureAwait(false);
        }
        
        private Task RunStartups(AthenaContext context)
        {
            lock (_syncRoot)
            {
                if (_running)
                    return Task.CompletedTask;

                _running = true;

                return Task.WhenAll(_startups.Select(x => x(context)));
            }
        }

        internal void WithSetup<TEvent>(Func<TPart, TEvent, AthenaSetupContext, Task> setup,
            Func<TEvent, bool> filter = null) where TEvent : SetupEvent
        {
            Logger.Write(LogLevel.Debug, $"Configuring setup for {typeof(TPart)} ({typeof(TEvent)})");
            
            filter = filter ?? (x => true);

            var fullFilter 
                = (Func<SetupEvent, bool>) (evnt => evnt.GetType() == typeof(TEvent) && filter((TEvent)evnt));

            _setups.Add(
                new Tuple<Func<SetupEvent, bool>, Func<object, SetupEvent, AthenaSetupContext, Task>>(fullFilter,
                    (part, evnt, context) => setup((TPart)part, (TEvent)evnt, context)));
        }
    }
}