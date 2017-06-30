using System;
using System.Threading.Tasks;

namespace Athena.Configuration
{
    public class PartConfiguration<TPart> : AthenaBootstrapper
    {
        private readonly AthenaBootstrapper _bootstrapper;
        private readonly TPart _part;

        public PartConfiguration(AthenaBootstrapper bootstrapper, TPart part)
        {
            _bootstrapper = bootstrapper;
            _part = part;
        }

        public string ApplicationName => _bootstrapper.ApplicationName;
        public string Environment => _bootstrapper.Environment;

        public AthenaBootstrapper WithApplicationName(string name)
        {
            return _bootstrapper.WithApplicationName(name);
        }

        public PartConfiguration<TPlugin> UsingPlugin<TPlugin>(TPlugin plugin) where TPlugin : class, AthenaPlugin
        {
            return _bootstrapper.UsingPlugin(plugin);
        }

        public BootstrapEventListenerSetup<TEvent> When<TEvent>() where TEvent : SetupEvent
        {
            return _bootstrapper.When<TEvent>();
        }

        public Task<AthenaContext> Build()
        {
            return _bootstrapper.Build();
        }

        public PartConfiguration<TPart> ConfigurePart(Action<TPart> configure)
        {
            configure(_part);

            return this;
        }
    }
}