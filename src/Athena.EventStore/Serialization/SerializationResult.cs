using System;

namespace Athena.EventStore.Serialization
{
    public class SerializationResult
    {
        public SerializationResult(Guid eventId, string type, bool isJson, byte[] data, byte[] metadata)
        {
            Metadata = metadata;
            Data = data;
            IsJson = isJson;
            Type = type;
            EventId = eventId;
        }

        public Guid EventId { get; }
        public string Type { get; }
        public bool IsJson { get; }
        public byte[] Data { get; }
        public byte[] Metadata { get; }
    }
}