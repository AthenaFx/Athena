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

        protected PartConfiguration(AthenaBootstrapper bootstrapper)
        {
            _bootstrapper = bootstrapper;
        }

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
        
        internal abstract object GetPart();
    }
    
    public class PartConfiguration<TPart> : PartConfiguration
    {
        private TPart _part;
        
        private readonly
            ConcurrentBag<Tuple<Func<SetupEvent, bool>, Func<object, SetupEvent, AthenaSetupContext, Task>>> _setups =
                new ConcurrentBag<Tuple<Func<SetupEvent, bool>, Func<object, SetupEvent, AthenaSetupContext, Task>>>()
            ;

        internal PartConfiguration(AthenaBootstrapper bootstrapper, TPart part) 
            : base(bootstrapper)
        {
            _part = part;
        }

        public PartConfiguration<TPart> UpdateSettings(Func<TPart, TPart> configure)
        {
            _part = configure(_part);

            return this;
        }
        
        internal override Task TrySetUp(SetupEvent evnt, AthenaSetupContext context)
        {
            return Task.WhenAll(_setups
                .Where(x => x.Item1(evnt))
                .Select(x => x.Item2(GetPart(), evnt, context)));
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

        internal override object GetPart()
        {
            return _part;
        }
    }
}