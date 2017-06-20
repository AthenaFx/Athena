using System;
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
            context.DefineApplication("esprojection", AppFunctions
                .StartWith(next => new Retry(next, 5, TimeSpan.FromSeconds(1), "Projection failed").Invoke)
                .Then(next => new HandleTransactions(next).Invoke)
                .Then(next => new SupplyMetaData(next).Invoke)
                .Then(next => new ExecuteProjection(next).Invoke)
                .Build(), false);
            
            return Task.CompletedTask;
        }

        public Task TearDown(AthenaBootstrapper context)
        {
            return Task.CompletedTask;
        }
    }
}