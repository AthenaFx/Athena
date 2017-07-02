using System.Threading.Tasks;
using Athena.Configuration;
using Athena.Processes;

namespace Athena.EventStore.StreamSubscriptions
{
    public static class SubscribersBootstrapExtensions
    {
        public static PartConfiguration<LiveSubscribersSettings> UseEventStoreStreamLiveSubscribers(
            this AthenaBootstrapper bootstrapper)
        {
            return bootstrapper
                .UseProcess(new RunStreamLiveSubscribers())
                .ConfigureWith<LiveSubscribersSettings>((config, context) =>
                {
                    context.DefineApplication(config.Name, config.GetApplicationBuilder());

                    return Task.CompletedTask;
                });
        }
        
        public static PartConfiguration<PersistentSubscribersSettings> UseEventStoreStreamPersistentSubscribers(
            this AthenaBootstrapper bootstrapper)
        {
            return bootstrapper
                .UseProcess(new RunStreamPersistentSubscribers())
                .ConfigureWith<PersistentSubscribersSettings>((config, context) =>
                {
                    context.DefineApplication(config.Name, config.GetApplicationBuilder());

                    return Task.CompletedTask;
                });
        }
    }
}