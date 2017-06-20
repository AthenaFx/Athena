using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Athena.Binding;
using Athena.EventStore.Serialization;

namespace Athena.EventStore.StreamSubscriptions
{
    public class EventDataBinder : EnvironmentDataBinder
    {
        public Task<DataBinderResult> Bind(Type to, IDictionary<string, object> environment)
        {
            var evnt = environment.Get<DeSerializationResult>("event");

            if (evnt == null)
                return Task.FromResult(new DataBinderResult(null, false));

            if (to == typeof(DeSerializationResult))
                return Task.FromResult(new DataBinderResult(evnt, true));

            if (to.IsInstanceOfType(evnt.Data))
                return Task.FromResult(new DataBinderResult(evnt.Data, true));
            
            return Task.FromResult(new DataBinderResult(null, false));
        }
    }
}