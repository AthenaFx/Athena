using System;
using System.Threading.Tasks;
using Athena.Configuration;

namespace Athena.Processes
{
    public static class LongRunningProcesses
    {
        public static AthenaBootstrapper UseProcess(this AthenaBootstrapper bootstrapper, 
            LongRunningProcess process)
        {
            return bootstrapper
                .ConfigureOn<BootstrapCompleted>(async (evnt, context) =>
                {
                    await process.Start(evnt.Context).ConfigureAwait(false);
                })
                .ShutDownWith(async context =>
                {
                    await process.Stop().ConfigureAwait(false);
                });
        }
        
        public static AthenaBootstrapper UseProcess(this AthenaBootstrapper bootstrapper, 
            LongRunningProcess process, Func<Func<bool, AthenaContext, Task>, Task> subscribeToChanges)
        {
            return bootstrapper.UseProcess(new ConditionedProcessWrapper(process, subscribeToChanges));
        }
    }
}