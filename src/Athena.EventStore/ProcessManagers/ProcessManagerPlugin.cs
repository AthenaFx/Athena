using System.Threading.Tasks;
using Athena.MetaData;
using Athena.Transactions;

namespace Athena.EventStore.ProcessManagers
{
    public class ProcessManagerPlugin : AthenaPlugin
    {
        public Task Bootstrap(AthenaBootstrapper context)
        {
            context.DefineApplication("esprocessmanager", AppFunctions
                .StartWith(next => new HandleTransactions(next).Invoke)
                .Then(next => new SupplyMetaData(next).Invoke)
                .Then(next => new ExecuteProcessManager(next).Invoke)
                .Build(), false);
            
            return Task.CompletedTask;
        }

        public Task TearDown(AthenaBootstrapper context)
        {
            return Task.CompletedTask;
        }
    }
}