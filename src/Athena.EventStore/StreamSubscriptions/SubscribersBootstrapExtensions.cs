using System.Threading.Tasks;
using Athena.Configuration;
using Athena.Logging;
using Athena.Processes;

namespace Athena.EventStore.StreamSubscriptions
{
    public static class SubscribersBootstrapExtensions
    {
        public static PartConfiguration<LiveSubscribersSettings> UseEventStoreStreamLiveSubscribers(
            this AthenaBootstrapper bootstrapper)
        {
            Logger.Write(LogLevel.Debug,
                $"Enabling EventStore live subscriptions for application {bootstrapper.ApplicationName}");
            
            return bootstrapper
                .UseProcess(new RunStreamLiveSubscribers())
                .ConfigureWith<LiveSubscribersSettings>((config, context) =>
                {
                    Logger.Write(LogLevel.Debug,
                        $"Configuring EventStore live subscriptions for application {context.ApplicationName}");
                    
                    context.DefineApplication(config.Name, config.GetApplicationBuilder());

                    return Task.CompletedTask;
                });
        }
        
        public static PartConfiguration<PersistentSubscribersSettings> UseEventStoreStreamPersistentSubscribers(
            this AthenaBootstrapper bootstrapper)
        {
            Logger.Write(LogLevel.Debug,
                $"Enabling EventStore persistent subscriptions for application {bootstrapper.ApplicationName}");
            
            return bootstrapper
                .UseProcess(new RunStreamPersistentSubscribers())
                .ConfigureWith<PersistentSubscribersSettings>((config, context) =>
                {
                    Logger.Write(LogLevel.Debug,
                        $"Configuring EventStore persistent subscriptions for application {context.ApplicationName}");
                    
                    context.DefineApplication(config.Name, config.GetApplicationBuilder());

                    return Task.CompletedTask;
                });
        }
    }
}