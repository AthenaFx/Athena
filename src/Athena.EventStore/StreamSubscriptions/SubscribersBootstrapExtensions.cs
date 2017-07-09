using Athena.Configuration;
using Athena.Logging;

namespace Athena.EventStore.StreamSubscriptions
{
    public static class SubscribersBootstrapExtensions
    {        
        public static PartConfiguration<RunStreamPersistentSubscribers> UseEventStoreStreamPersistentSubscribers(
            this AthenaBootstrapper bootstrapper)
        {
            Logger.Write(LogLevel.Debug,
                $"Enabling EventStore persistent subscriptions for application {bootstrapper.ApplicationName}");
            
            return bootstrapper
                .Part<RunStreamPersistentSubscribers>()
                .OnSetup(async (config, context) =>
                {
                    Logger.Write(LogLevel.Debug,
                        $"Configuring EventStore persistent subscriptions for application {context.ApplicationName}");
                    
                    await context.DefineApplication(config.Name, config.GetApplicationBuilder()).ConfigureAwait(false);
                })
                .OnStartup((config, context) => config.Start(context))
                .OnShutdown((config, context) => config.Stop());
        }
    }
}