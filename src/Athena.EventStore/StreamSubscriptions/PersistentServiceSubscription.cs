using System;
using EventStore.ClientAPI;

namespace Athena.EventStore.StreamSubscriptions
{
    public class PersistentServiceSubscription : IServiceSubscription
    {
        private readonly EventStorePersistentSubscriptionBase _eventStoreSubscription;

        public PersistentServiceSubscription(EventStorePersistentSubscriptionBase eventStoreSubscription)
        {
            _eventStoreSubscription = eventStoreSubscription;
        }

        public bool Close()
        {
            try
            {
                _eventStoreSubscription.Stop(TimeSpan.FromSeconds(5));

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}