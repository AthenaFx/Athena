using System;
using System.Collections.Generic;
using System.Linq;
using Athena.Configuration;
using Athena.EventStore.Serialization;
using Athena.MetaData;
using Athena.Transactions;

namespace Athena.EventStore.Projections
{
    public class ProjectionSettings : AppFunctionDefinition
    {
        private readonly ICollection<Transaction> _transactions = new List<Transaction>();
        private readonly ICollection<EventStoreProjection> _projections = new List<EventStoreProjection>();
        private readonly ICollection<MetaDataSupplier> _metaDataSuppliers = new List<MetaDataSupplier>();
        private EventSerializer _serializer = new JsonEventSerializer();
        private EventStoreConnectionString _connectionString 
            = new EventStoreConnectionString("Ip=127.0.0.1;Port=1113;UserName=admin;Password=changeit;");

        private ProjectionsPositionHandler _positionHandler = new StoreProjectionsPositionOnDisc();

        public ProjectionSettings HandleTransactionsWith(Transaction transaction)
        {
            _transactions.Add(transaction);

            return this;
        }

        public ProjectionSettings SupplyMetaDataWith(MetaDataSupplier supplier)
        {
            _metaDataSuppliers.Add(supplier);

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

        public string Name { get; } = "esprojection";
        
        protected override AppFunctionBuilder DefineDefaultApplication(AppFunctionBuilder builder)
        {
            return builder
                .Last("Retry", next => new Retry(next, 5, TimeSpan.FromSeconds(1), "Projection failed").Invoke)
                .Last("HandleTransactions",
                    next => new HandleTransactions(next, _transactions.ToList()).Invoke)
                .Last("SupplyMetaData", next => new SupplyMetaData(next, _metaDataSuppliers.ToList()).Invoke)
                .Last("ExecuteProjection", next => new ExecuteProjection(next).Invoke);
        }
    }
}