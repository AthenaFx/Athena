using System;
using System.Linq;
using System.Threading.Tasks;
using Athena.MetaData;
using Athena.Processes;
using Athena.Transactions;

namespace Athena.EventStore.Projections
{
    public class ProjectionsPlugin : AthenaPlugin
    {
        public Task Bootstrap(AthenaBootstrapper context)
        {
            context.DefineApplication("esprojection", builder => builder
                .Last("Retry", next => new Retry(next, 5, TimeSpan.FromSeconds(1), "Projection failed").Invoke)
                .Last("HandleTransactions", next => new HandleTransactions(next, Enumerable.Empty<Transaction>()).Invoke)
                .Last("SupplyMetaData", next => new SupplyMetaData(next).Invoke)
                .Last("ExecuteProjection", next => new ExecuteProjection(next).Invoke), false);
            
            return Task.CompletedTask;
        }

        public Task TearDown(AthenaBootstrapper context)
        {
            return Task.CompletedTask;
        }
    }
}