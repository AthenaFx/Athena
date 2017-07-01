using System;
using System.Threading.Tasks;
using Athena.Configuration;
using Athena.EventStore.Serialization;
using Athena.Processes;

namespace Athena.EventStore.StreamSubscriptions
{
    public static class SubscribersBootstrapExtensions
    {
        public static PartConfiguration<SubscribersSettings> UseEventStoreStreamSubscribers(
            this AthenaBootstrapper bootstrapper)
        {
            return bootstrapper
                .UseProcess(new RunStreamSubscribers())
                .ConfigureWith<SubscribersSettings>((config, context) =>
                {
                    context.DefineApplication("livesubscription", config.GetLiveSubscriptionBuilder());
            
                    context.DefineApplication("persistentsubscription", config.GetPersistensSubscriptionBuilder());

                    return Task.CompletedTask;
                });
        }

        public static PartConfiguration<SubscribersSettings> WithSerializer(
            this PartConfiguration<SubscribersSettings> config, EventSerializer serializer)
        {
            return config.UpdateSettings(x => x.WithSerializer(serializer));
        }

        public static PartConfiguration<SubscribersSettings> WithConnectionString(
            this PartConfiguration<SubscribersSettings> config, string connectionString)
        {
            return config.UpdateSettings(x => x.WithConnectionString(connectionString));
        }
        
        public static PartConfiguration<SubscribersSettings> ConfigureLiveSubscriptionApplication(
            this PartConfiguration<SubscribersSettings> config, 
            Func<AppFunctionBuilder, AppFunctionBuilder> configure)
        {
            return config.UpdateSettings(x => x.ConfigureLiveSubscriptionApplication(configure));
        }
        
        public static PartConfiguration<SubscribersSettings> ConfigurePersistensSubscriptionApplication(
            this PartConfiguration<SubscribersSettings> config, 
            Func<AppFunctionBuilder, AppFunctionBuilder> configure)
        {
            return config.UpdateSettings(x => x.ConfigurePersistentSubscriptionApplication(configure));
        }
    }
}