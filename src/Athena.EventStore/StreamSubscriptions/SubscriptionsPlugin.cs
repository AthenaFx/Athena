using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Binding;
using Athena.MetaData;
using Athena.Resources;
using Athena.Routing;
using Athena.Transactions;

namespace Athena.EventStore.StreamSubscriptions
{
    public class SubscriptionsPlugin : AthenaPlugin
    {
        public Task Bootstrap(AthenaBootstrapper context)
        {
            var routers = new List<EnvironmentRouter>
            {
                RouteEventToMethod.New(x => x.Name == "Subscribe"
                                              && (x.ReturnType == typeof(void) || x.ReturnType == typeof(Task))
                                              && x.GetParameters().Length == 1)
            }.AsReadOnly();

            var binders = new List<EnvironmentDataBinder>
            {
                new BindEnvironment(),
                new EventDataBinder()
            }.AsReadOnly();

            var resourceExecutors = new List<ResourceExecutor>
            {
                new MethodResourceExecutor(binders)
            };
            
            context.DefineApplication("livesubscription", AppFunctions
                .StartWith(next => new Retry(next, 5, TimeSpan.FromSeconds(1), "Subscription failed").Invoke)
                .Then(next => new HandleTransactions(next).Invoke)
                .Then(next => new SupplyMetaData(next).Invoke)
                .Then(next => new RouteToResource(next, routers).Invoke)
                .Then(next => new ExecuteResource(next, resourceExecutors).Invoke)
                .Build());
            
            context.DefineApplication("persistentsubscription", AppFunctions
                .StartWith(next => new HandleTransactions(next).Invoke)
                .Then(next => new SupplyMetaData(next).Invoke)
                .Then(next => new RouteToResource(next, routers).Invoke)
                .Then(next => new ExecuteResource(next, resourceExecutors).Invoke)
                .Build());
            
            return Task.CompletedTask;
        }

        public Task TearDown(AthenaBootstrapper context)
        {
            return Task.CompletedTask;
        }
    }
}