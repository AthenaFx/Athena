using System;
using System.Collections.Generic;
using System.Linq;
using Athena.Configuration;
using Athena.EventStore.Serialization;
using Athena.MetaData;
using Athena.Transactions;

namespace Athena.EventStore.Projections
{
    public class ProjectionSettings
    {
        private Func<AppFunctionBuilder, AppFunctionBuilder> _builder = builder => builder
            .Last("Retry", next => new Retry(next, 5, TimeSpan.FromSeconds(1), "Projection failed").Invoke)
            .Last("HandleTransactions", next => new HandleTransactions(next, Enumerable.Empty<Transaction>()).Invoke)
            .Last("SupplyMetaData", next => new SupplyMetaData(next).Invoke)
            .Last("ExecuteProjection", next => new ExecuteProjection(next).Invoke);
        
        private readonly ICollection<EventStoreProjection> _projections = new List<EventStoreProjection>();
        private EventSerializer _serializer = new JsonEventSerializer();
        private EventStoreConnectionString _connectionString 
            = new EventStoreConnectionString("Ip=127.0.0.1;Port=1113;UserName=admin;Password=changeit;");

        private ProjectionsPositionHandler _positionHandler = new StoreProjectionsPositionOnDisc();

        public ProjectionSettings ConfigureApplication(Func<AppFunctionBuilder, AppFunctionBuilder> configure)
        {
            var currentBuilder = _builder;

            _builder = (builder => configure(currentBuilder(builder)));

            return this;
        }
        
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

        public ProjectionSettings WithPositionHandler(ProjectionsPositionHandler positionHandler)
        {
            _positionHandler = positionHandler;

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

        internal IReadOnlyCollection<EventStoreProjection> GetProjections()
        {
            return _projections.ToList();
        }

        internal ProjectionsPositionHandler GetPositionHandler()
        {
            return _positionHandler;
        }
        
        internal Func<AppFunctionBuilder, AppFunctionBuilder> GetBuilder()
        {
            return _builder;
        }
    }
}