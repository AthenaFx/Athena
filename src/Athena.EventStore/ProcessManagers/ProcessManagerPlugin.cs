using System.Linq;
using System.Threading.Tasks;
using Athena.MetaData;
using Athena.Transactions;

namespace Athena.EventStore.ProcessManagers
{
    public class ProcessManagerPlugin : AthenaPlugin
    {
        public Task Bootstrap(AthenaBootstrapper context)
        {
            context.DefineApplication("esprocessmanager", builder => builder
                .Last("HandleTransactions", next => new HandleTransactions(next, Enumerable.Empty<Transaction>()).Invoke)
                .Last("SupplyMetaData", next => new SupplyMetaData(next).Invoke)
                .Last("ExecuteProcessManager", next => new ExecuteProcessManager(next).Invoke), false);
            
            return Task.CompletedTask;
        }

        public Task TearDown(AthenaBootstrapper context)
        {
            return Task.CompletedTask;
        }
    }
}