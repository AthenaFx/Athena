using System;
using System.Collections.Generic;
using System.Text;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace Athena.EventStore.Serialization
{
    public class JsonEventSerializer : EventSerializer
    {
        private static readonly JsonSerializerSettings SerializerSettings;
        private const string EventClrTypeHeader = "EventClrTypeName";

        static JsonEventSerializer()
        {
            SerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None
            };
        }

        public SerializationResult Serialize(Guid eventId, object evnt, IDictionary<string, object> headers)
        {
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(evnt, SerializerSettings));

            var eventHeaders = new Dictionary<string, object>(headers)
            {
                [EventClrTypeHeader] = evnt.GetType().AssemblyQualifiedName
            };

            var metadata = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(eventHeaders, SerializerSettings));
            var typeName = evnt.GetType().Name;

            return new SerializationResult(eventId, typeName, true, data, metadata);
        }

        public DeSerializationResult DeSerialize(ResolvedEvent resolvedEvent)
        {
            try
            {
                var deSerializedMetaData = (Dictionary<string, object>)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(resolvedEvent.Event.Metadata), typeof(Dictionary<string, object>));

                var deSerializedData = deSerializedMetaData.ContainsKey(EventClrTypeHeader) ? JsonConvert.DeserializeObject(Encoding.UTF8.GetString(resolvedEvent.Event.Data), 
                    Type.GetType((string)deSerializedMetaData[EventClrTypeHeader])) : new EventStoreEvent();

                return new DeSerializationResult(deSerializedData, deSerializedMetaData, resolvedEvent);
            }
            catch (Exception exception)
            {
                return new DeSerializationResult(null, new Dictionary<string, object>(), resolvedEvent, exception);
            }
        }

        public class EventStoreEvent
        {

        }
    }
}