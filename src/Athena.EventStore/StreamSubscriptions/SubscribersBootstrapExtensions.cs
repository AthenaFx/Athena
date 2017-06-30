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
            var settings = new SubscribersSettings();
            
            bootstrapper = bootstrapper
                .UsingPlugin(new SubscriptionsPlugin())
                .UseProcess(new RunStreamSubscribers(settings));
            
            return new PartConfiguration<SubscribersSettings>(bootstrapper, settings);
        }

        public static PartConfiguration<SubscribersSettings> WithSerializer(
            this PartConfiguration<SubscribersSettings> config, EventSerializer serializer)
        {
            return config.ConfigurePart(x => x.WithSerializer(serializer));
        }

        public static PartConfiguration<SubscribersSettings> WithConnectionString(
            this PartConfiguration<SubscribersSettings> config, string connectionString)
        {
            return config.ConfigurePart(x => x.WithConnectionString(connectionString));
        }
    }
}