using System.Collections.Generic;
using System.Linq;
using Athena.EventStore.Serialization;

namespace Athena.EventStore.Projections
{
    public class ProjectionSettings
    {
        private readonly ICollection<EventStoreProjection> _projections = new List<EventStoreProjection>();
        private EventSerializer _serializer = new JsonEventSerializer();
        private EventStoreConnectionString _connectionString 
            = new EventStoreConnectionString("Ip=127.0.0.1;Port=1113;UserName=admin;Password=changeit;");

        public ProjectionSettings WithSerializer(EventSerializer serializer)
        {
            _serializer = serializer;

            return this;
        }

        public ProjectionSettings WithConnectionString(string connectionString)
        {
            _connectionString = new EventStoreConnectionString(connectionString);

            return this;
        }

        public ProjectionSettings WithProjection(EventStoreProjection projection)
        {
            _projections.Add(projection);

            return this;
        }

        public EventSerializer GetSerializer()
        {
            return _serializer;
        }

        public EventStoreConnectionString GetConnectionString()
        {
            return _connectionString;
        }

        public IReadOnlyCollection<EventStoreProjection> GetProjections()
        {
            return _projections.ToList();
        }
    }
}