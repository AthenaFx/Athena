using System;
using System.Collections.Generic;
using EventStore.ClientAPI;

namespace Athena.EventStore.Serialization
{
    public class DeSerializationResult
    {
        public DeSerializationResult(object data, Dictionary<string, object> metadata, 
            ResolvedEvent originalEvent, Exception error = null)
        {
            Error = error;
            Metadata = metadata;
            OriginalEvent = originalEvent;
            Data = data;
        }

        public object Data { get; }
        public Dictionary<string, object> Metadata { get; }
        public ResolvedEvent OriginalEvent { get; }
        public Exception Error { get; }
        public bool Successful => Error == null;
    }
}