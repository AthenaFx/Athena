using System;
using Athena.Configuration;
using Athena.EventStore.Serialization;
using Athena.Processes;
using Athena.Settings;

namespace Athena.EventStore.StreamSubscriptions
{
    public static class SubscribersBootstrapExtensions
    {
        //TODO:Expand with better config options
        public static AthenaBootstrapper UseEventStoreStreamSubscribers(this AthenaBootstrapper bootstrapper,
            EventStoreConnectionString connectionString, Func<SubscribersSettings, SubscribersSettings> alterSettings,
            EventSerializer serializer = null)
        {
            serializer = serializer ?? new JsonEventSerializer();
            return bootstrapper
                .AlterSettings(alterSettings)
                .UsingPlugin(new SubscriptionsPlugin())
                .UseProcess(new RunStreamSubscribers(connectionString, serializer));
        }
    }
}