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
    public class SubscribersSettings
    {
        private Func<AppFunctionBuilder, AppFunctionBuilder> _liveSubscriptionsBuilder = builder =>
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
            
            return builder
                .Last("Retry", next => new Retry(next, 5, TimeSpan.FromSeconds(1), "Subscription failed").Invoke)
                .Last("HandleTransactions", next => new HandleTransactions(next, Enumerable.Empty<Transaction>()).Invoke)
                .Last("SupplyMetaData", next => new SupplyMetaData(next).Invoke)
                .Last("RouteToResource", next => new RouteToResource(next, routers).Invoke)
                .Last("ExecuteResource", next => new ExecuteResource(next, resourceExecutors).Invoke);
        };
        
        private Func<AppFunctionBuilder, AppFunctionBuilder> _persistentSubscriptionsBuilder = builder =>
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
            
            return builder
                .Last("HandleTransactions", next => new HandleTransactions(next, Enumerable.Empty<Transaction>()).Invoke)
                .Last("SupplyMetaData", next => new SupplyMetaData(next).Invoke)
                .Last("RouteToResource", next => new RouteToResource(next, routers).Invoke)
                .Last("ExecuteResource", next => new ExecuteResource(next, resourceExecutors).Invoke);
        };
        
        private readonly ICollection<Tuple<string, int, bool>> _streams = new List<Tuple<string, int, bool>>();
        private EventSerializer _serializer = new JsonEventSerializer();
        private EventStoreConnectionString _connectionString 
            = new EventStoreConnectionString("Ip=127.0.0.1;Port=1113;UserName=admin;Password=changeit;");
        
        public SubscribersSettings ConfigureLiveSubscriptionApplication(Func<AppFunctionBuilder, AppFunctionBuilder> configure)
        {
            var currentBuilder = _liveSubscriptionsBuilder;

            _liveSubscriptionsBuilder = (builder => configure(currentBuilder(builder)));

            return this;
        }
        
        public SubscribersSettings ConfigurePersistentSubscriptionApplication(Func<AppFunctionBuilder, AppFunctionBuilder> configure)
        {
            var currentBuilder = _persistentSubscriptionsBuilder;

            _persistentSubscriptionsBuilder = (builder => configure(currentBuilder(builder)));

            return this;
        }

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
        
        internal Func<AppFunctionBuilder, AppFunctionBuilder> GetLiveSubscriptionBuilder()
        {
            return _liveSubscriptionsBuilder;
        }
        
        internal Func<AppFunctionBuilder, AppFunctionBuilder> GetPersistensSubscriptionBuilder()
        {
            return _persistentSubscriptionsBuilder;
        }
    }
}