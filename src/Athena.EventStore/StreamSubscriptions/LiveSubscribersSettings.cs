using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Athena.Binding;
using Athena.Configuration;
using Athena.EventStore.Serialization;
using Athena.MetaData;
using Athena.Resources;
using Athena.Routing;
using Athena.Transactions;

namespace Athena.EventStore.StreamSubscriptions
{
    public class LiveSubscribersSettings : AppFunctionDefinition
    {
        private readonly ICollection<Transaction> _transactions = new List<Transaction>();
        private readonly ICollection<Tuple<string, int>> _streams = new List<Tuple<string, int>>();
        private EventSerializer _serializer = new JsonEventSerializer();
        private EventStoreConnectionString _connectionString 
            = new EventStoreConnectionString("Ip=127.0.0.1;Port=1113;UserName=admin;Password=changeit;");
        
        public LiveSubscribersSettings HandleTransactionsWith(Transaction transaction)
        {
            _transactions.Add(transaction);

            return this;
        }

        public LiveSubscribersSettings SubscribeToStream(string stream, int workers = 1)
        {
            _streams.Add(new Tuple<string, int>(stream, workers));

            return this;
        }

        public LiveSubscribersSettings WithSerializer(EventSerializer serializer)
        {
            _serializer = serializer;

            return this;
        }

        public LiveSubscribersSettings WithConnectionString(string connectionString)
        {
            _connectionString = new EventStoreConnectionString(connectionString);

            return this;
        }

        public IReadOnlyCollection<Tuple<string, int>> GetSubscribedStreams()
        {
            return new ReadOnlyCollection<Tuple<string, int>>(_streams.ToList());
        }

        public EventSerializer GetSerializer()
        {
            return _serializer;
        }

        public EventStoreConnectionString GetConnectionString()
        {
            return _connectionString;
        }

        public string Name { get; } = "eslivesubscription";
        
        protected override AppFunctionBuilder DefineDefaultApplication(AppFunctionBuilder builder)
        {
            var routers = new List<EnvironmentRouter>
            {
                RouteEventToMethod.New(x => x.Name == "Subscribe"
                                            && (x.ReturnType == typeof(void) || x.ReturnType == typeof(Task))
                                            && x.GetParameters().Length == 1, builder.Bootstrapper.ApplicationAssemblies)
            };

            var binders = new List<EnvironmentDataBinder>
            {
                new BindEnvironment(),
                new EventDataBinder()
            };

            var resourceExecutors = new List<ResourceExecutor>
            {
                new MethodResourceExecutor(binders)
            };

            //TODO:Make sure we can have multiple subscribers to same event
            return builder
                .Last("Retry", next => new Retry(next, 5, TimeSpan.FromSeconds(1), "Subscription failed").Invoke)
                .Last("HandleTransactions",
                    next => new HandleTransactions(next, _transactions.ToList()).Invoke)
                .Last("SupplyMetaData", next => new SupplyMetaData(next).Invoke)
                .Last("RouteToResource", next => new RouteToResource(next, routers).Invoke)
                .Last("ExecuteResource", next => new ExecuteResource(next, resourceExecutors).Invoke);
        }
    }
}