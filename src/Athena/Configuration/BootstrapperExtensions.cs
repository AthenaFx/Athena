using System;
using System.Threading.Tasks;

namespace Athena.Configuration
{
    public static class BootstrapperExtensions
    {
        public static AthenaBootstrapper ConfigureWith(this AthenaBootstrapper bootstrapper, 
            Func<AthenaSetupContext, Task> setup)
        {
            var key = Guid.NewGuid().ToString("N");
            
            return bootstrapper.ConfigureWith<object>((part, context) => setup(context), key);
        }
        
        public static AthenaBootstrapper ConfigureOn<TEvent>(this AthenaBootstrapper bootstrapper, 
            Func<TEvent, AthenaSetupContext, Task> setup) where TEvent : SetupEvent
        {
            var key = Guid.NewGuid().ToString("N");
            
            return bootstrapper.ConfigureWith<object, TEvent>(
                (part, evnt, context) => setup(evnt, context), x => true, key);
        }
        
        public static PartConfiguration<TPart> ConfigureWith<TPart>(this AthenaBootstrapper bootstrapper, 
            Func<TPart, AthenaSetupContext, Task> setup, string key = null) where TPart : class, new()
        {
            return bootstrapper.ConfigureWith<TPart, BootstrapStarted>((part, evnt, context) => setup(part, context),
                x => true, key);
        }

        public static AthenaBootstrapper ShutDownWith(this AthenaBootstrapper bootstrapper, 
            Func<AthenaContext, Task> shutDown)
        {
            return bootstrapper.ShutDownWith<ShutdownStarted>((evnt, context) => shutDown(context));
        }
    }
}