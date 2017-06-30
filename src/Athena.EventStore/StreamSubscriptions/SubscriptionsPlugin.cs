using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Athena.Binding;
using Athena.Configuration;
using Athena.MetaData;
using Athena.Resources;
using Athena.Routing;
using Athena.Transactions;

namespace Athena.EventStore.StreamSubscriptions
{
    public class SubscriptionsPlugin : AthenaPlugin
    {
        public Task Bootstrap(AthenaSetupContext context)
        {
            var routers = new List<EnvironmentRouter>
            {
                RouteEventToMethod.New(x => x.Name == "Subscribe"
                                              && (x.ReturnType == typeof(void) || x.ReturnType == typeof(Task))
                                              && x.GetParameters().Length == 1, context.ApplicationAssemblies)
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
            
            context.DefineApplication("livesubscription", builder => builder
                .Last("Retry", next => new Retry(next, 5, TimeSpan.FromSeconds(1), "Subscription failed").Invoke)
                .Last("HandleTransactions", next => new HandleTransactions(next, Enumerable.Empty<Transaction>()).Invoke)
                .Last("SupplyMetaData", next => new SupplyMetaData(next).Invoke)
                .Last("RouteToResource", next => new RouteToResource(next, routers).Invoke)
                .Last("ExecuteResource", next => new ExecuteResource(next, resourceExecutors).Invoke));
            
            context.DefineApplication("persistentsubscription", builder => builder
                .Last("HandleTransactions", next => new HandleTransactions(next, Enumerable.Empty<Transaction>()).Invoke)
                .Last("SupplyMetaData", next => new SupplyMetaData(next).Invoke)
                .Last("RouteToResource", next => new RouteToResource(next, routers).Invoke)
                .Last("ExecuteResource", next => new ExecuteResource(next, resourceExecutors).Invoke));
            
            return Task.CompletedTask;
        }

        public Task TearDown(AthenaContext context)
        {
            return Task.CompletedTask;
        }
    }
}