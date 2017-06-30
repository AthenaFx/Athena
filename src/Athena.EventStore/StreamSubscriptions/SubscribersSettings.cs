using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Athena.EventStore.Serialization;

namespace Athena.EventStore.StreamSubscriptions
{
    public class SubscribersSettings
    {
        private readonly ICollection<Tuple<string, int, bool>> _streams = new List<Tuple<string, int, bool>>();
        private EventSerializer _serializer = new JsonEventSerializer();
        private EventStoreConnectionString _connectionString 
            = new EventStoreConnectionString("Ip=127.0.0.1;Port=1113;UserName=admin;Password=changeit;");

        public SubscribersSettings SubscribeToStream(string stream, int workers = 1, bool liveOnly = false)
        {
            _streams.Add(new Tuple<string, int, bool>(stream, workers, liveOnly));

            return this;
        }

        public SubscribersSettings WithSerializer(EventSerializer serializer)
        {
            _serializer = serializer;

            return this;
        }

        public SubscribersSettings WithConnectionString(string connectionString)
        {
            _connectionString = new EventStoreConnectionString(connectionString);

            return this;
        }

        public IReadOnlyCollection<Tuple<string, int, bool>> GetSubscribedStreams()
        {
            return new ReadOnlyCollection<Tuple<string, int, bool>>(_streams.ToList());
        }

        public EventSerializer GetSerializer()
        {
            return _serializer;
        }

        public EventStoreConnectionString GetConnectionString()
        {
            return _connectionString;
        }
    }
}