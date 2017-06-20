using System.Threading.Tasks;
using Athena.Messages;
using Athena.PubSub;

namespace Athena.Processes
{
    public class ProcessPlugin : AthenaPlugin
    {
        public Task Bootstrap(AthenaBootstrapper context)
        {
            EventPublishing.Subscribe<BootstrapCompleted>(async x => await LongRunningProcesses.StartAllProcesses());

            return Task.CompletedTask;
        }

        public Task TearDown(AthenaBootstrapper context)
        {
            return LongRunningProcesses.StopAllProcesses();
        }
    }
}