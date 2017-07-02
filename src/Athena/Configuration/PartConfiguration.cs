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

        private Func<object, object, AthenaSetupContext, Task<object>> _configureParent =
            (x, y, z) => Task.FromResult(x);
        
        protected readonly ConcurrentBag<PartConfiguration> Children = new ConcurrentBag<PartConfiguration>();

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

        internal void ConfigureParentWith(Func<object, object, AthenaSetupContext, Task<object>> config)
        {
            _configureParent = config;
        }

        internal async Task<object> ConfigureParent(object parent, AthenaSetupContext context)
        {
            var me = GetPart();

            foreach (var child in Children)
                me = await child.ConfigureParent(me, context).ConfigureAwait(false);

            return await _configureParent(parent, me, context).ConfigureAwait(false);
        }

        internal abstract object GetPart();
    }
    
    public class PartConfiguration<TPart> : PartConfiguration where TPart : class, new()
    {
        private TPart _part;
        private bool _hasSetup;
        private readonly AthenaBootstrapper _bootstrapper;
        
        private readonly
            ConcurrentBag<Tuple<Func<SetupEvent, bool>, Func<object, SetupEvent, AthenaSetupContext, Task>>> _setups =
                new ConcurrentBag<Tuple<Func<SetupEvent, bool>, Func<object, SetupEvent, AthenaSetupContext, Task>>>();

        internal PartConfiguration(AthenaBootstrapper bootstrapper, string key) 
            : base(bootstrapper, key)
        {
            _bootstrapper = bootstrapper;
            _part = new TPart();
        }

        public PartConfiguration<TPart> Configure(Func<TPart, TPart> configure)
        {
            _part = configure(_part);

            return this;
        }

        public PartConfiguration<TChild> Child<TChild>(
            Func<TPart, TChild, AthenaSetupContext, Task<TPart>> configureParent, string key = null) 
            where TChild : class, new()
        {
            var child = _bootstrapper.ConfigureWith<TChild>(key);
            child.ConfigureParentWith(async (parent, me, context) => 
                await configureParent((TPart) parent, (TChild) me, context).ConfigureAwait(false));

            Children.Add(child);

            return child;
        }
        
        public PartConfiguration<TChild> Child<TChild>(
            Func<TPart, TChild, AthenaSetupContext, TPart> configureParent, string key = null) 
            where TChild : class, new()
        {
            return Child<TChild>((parent, child, context) => 
                Task.FromResult(configureParent(parent, child, context)), key);
        }
        
        internal override async Task TrySetUp(SetupEvent evnt, AthenaSetupContext context)
        {
            var availableSetups = _setups
                .Where(x => x.Item1(evnt))
                .ToList();
            
            if(!availableSetups.Any())
                return;

            if (!_hasSetup)
            {
                foreach (var child in Children)
                    _part = (TPart) (await child.ConfigureParent(_part, context).ConfigureAwait(false));
            }
            
            _hasSetup = true;

            var part = GetPart();
            
            await Task.WhenAll(availableSetups
                .Select(x => x.Item2(part, evnt, context)))
                .ConfigureAwait(false);
        }

        internal override object GetPart()
        {
            return _part;
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