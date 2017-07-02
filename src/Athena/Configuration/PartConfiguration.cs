using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Athena.Configuration
{
    public abstract class PartConfiguration : AthenaBootstrapper
    {
        private readonly AthenaBootstrapper _bootstrapper;
        private Func<object, object> _configureParent = x => x;

        protected PartConfiguration(AthenaBootstrapper bootstrapper, string key)
        {
            _bootstrapper = bootstrapper;
            Key = key;
        }

        public string Key { get; }
        public string ApplicationName => _bootstrapper.ApplicationName;
        public string Environment => _bootstrapper.Environment;
        
        public TSetting GetSetting<TSetting>(string key = null) where TSetting : class
        {
            return _bootstrapper.GetSetting<TSetting>(key);
        }

        public IReadOnlyCollection<Assembly> ApplicationAssemblies => _bootstrapper.ApplicationAssemblies;

        public PartConfiguration<TPart> Configure<TPart>(string key = null) where TPart : class, new()
        {
            return _bootstrapper.Configure<TPart>(key);
        }

        public PartConfiguration<TNewPart> ConfigureWith<TNewPart, TEvent>(
            Func<TNewPart, TEvent, AthenaSetupContext, Task> setup, 
            Func<TEvent, bool> filter = null, string key = null) where TNewPart : class, new() where TEvent : SetupEvent
        {
            return _bootstrapper.ConfigureWith(setup, filter, key);
        }

        public PartConfiguration<TPart> ConfigureWith<TPart>(string key = null) where TPart : class, new()
        {
            return _bootstrapper.ConfigureWith<TPart>(key);
        }

        public AthenaBootstrapper ShutDownWith<TEvent>(Func<TEvent, AthenaContext, Task> shutDown,
            Func<TEvent, bool> filter = null) 
            where TEvent : ShutdownEvent
        {
            return _bootstrapper.ShutDownWith(shutDown, filter);
        }

        public Task<AthenaContext> Build()
        {
            return _bootstrapper.Build();
        }

        internal abstract Task TrySetUp(SetupEvent evnt, AthenaSetupContext context);

        internal void ConfigureParentWith(Func<object, object> config)
        {
            _configureParent = config;
        }

        internal object ConfigureParent(object parent)
        {
            return _configureParent(parent);
        }

        internal abstract object GetPart();
    }
    
    public class PartConfiguration<TPart> : PartConfiguration where TPart : class, new()
    {
        private bool _hasSetup;
        private readonly AthenaBootstrapper _bootstrapper;
        private readonly ConcurrentBag<Func<TPart, TPart>> _configurations = new ConcurrentBag<Func<TPart, TPart>>();
        private readonly ConcurrentBag<PartConfiguration> _children = new ConcurrentBag<PartConfiguration>();
        
        private readonly
            ConcurrentBag<Tuple<Func<SetupEvent, bool>, Func<object, SetupEvent, AthenaSetupContext, Task>>> _setups =
                new ConcurrentBag<Tuple<Func<SetupEvent, bool>, Func<object, SetupEvent, AthenaSetupContext, Task>>>()
            ;

        internal PartConfiguration(AthenaBootstrapper bootstrapper, string key) 
            : base(bootstrapper, key)
        {
            _bootstrapper = bootstrapper;
        }

        public PartConfiguration<TPart> Configure(Func<TPart, TPart> configure)
        {
            _configurations.Add(configure);

            return this;
        }

        public PartConfiguration<TChild> Child<TChild>(Func<TPart, TChild, TPart> configureParent, string key = null) 
            where TChild : class, new()
        {
            var existingChild = _children.OfType<PartConfiguration<TChild>>().FirstOrDefault();

            if (existingChild != null)
                return existingChild;
            
            var child = _bootstrapper.ConfigureWith<TChild>(key);
            child.ConfigureParentWith(parent => configureParent((TPart)parent, (TChild)child.GetPart()));

            _children.Add(child);

            return child;
        }
        
        internal override async Task TrySetUp(SetupEvent evnt, AthenaSetupContext context)
        {
            _hasSetup = true;

            var part = GetPart();
            
            await Task.WhenAll(_setups
                .Where(x => x.Item1(evnt))
                .Select(x => x.Item2(part, evnt, context)))
                .ConfigureAwait(false);
        }

        internal override object GetPart()
        {
            var part = _configurations.Aggregate(new TPart(), (current, configuration) => configuration(current));

            if (_hasSetup)
            {
                part = _children.Aggregate(part, (current, child) =>
                    (TPart) child.ConfigureParent(current));
            }

            return part;
        }

        internal void WithSetup<TEvent>(Func<TPart, TEvent, AthenaSetupContext, Task> setup,
            Func<TEvent, bool> filter = null) where TEvent : SetupEvent
        {
            filter = filter ?? (x => true);

            var fullFilter 
                = (Func<SetupEvent, bool>) (evnt => evnt.GetType() == typeof(TEvent) && filter((TEvent)evnt));

            _setups.Add(
                new Tuple<Func<SetupEvent, bool>, Func<object, SetupEvent, AthenaSetupContext, Task>>(fullFilter,
                    (part, evnt, context) => setup((TPart)part, (TEvent)evnt, context)));
        }
    }
}