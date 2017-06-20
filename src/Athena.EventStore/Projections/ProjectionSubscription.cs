using System;
using EventStore.ClientAPI;

namespace Athena.EventStore.Projections
{
    public class ProjectionSubscription
    {
        private readonly IDisposable _observableSubscription;
        private readonly IDisposable _setLastEventSubscription;
        private readonly EventStoreStreamCatchUpSubscription _eventStoreSubscription;

        public ProjectionSubscription(IDisposable observableSubscription, IDisposable setLastEventSubscription, 
            EventStoreStreamCatchUpSubscription eventStoreSubscription)
        {
            _observableSubscription = observableSubscription;
            _eventStoreSubscription = eventStoreSubscription;
            _setLastEventSubscription = setLastEventSubscription;
        }

        public bool Close()
        {
            try
            {
                _eventStoreSubscription.Stop(TimeSpan.FromSeconds(5));
                _observableSubscription.Dispose();
                _setLastEventSubscription.Dispose();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}