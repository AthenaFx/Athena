using System;
using EventStore.ClientAPI;

namespace Athena.EventStore.ProcessManagers
{
    public class ProcessManagerSubscription
    {
        private readonly EventStorePersistentSubscriptionBase _eventStoreSubscription;

        public ProcessManagerSubscription(EventStorePersistentSubscriptionBase eventStoreSubscription)
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