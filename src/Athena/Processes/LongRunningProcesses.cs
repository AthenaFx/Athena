using System;
using System.Threading.Tasks;
using Athena.Configuration;
using Athena.Logging;

namespace Athena.Processes
{
    public static class LongRunningProcesses
    {
        public static AthenaBootstrapper UseProcess(this AthenaBootstrapper bootstrapper, 
            LongRunningProcess process)
        {
            Logger.Write(LogLevel.Debug, $"Adding a long running process {process}");
            
            return bootstrapper
                .ConfigureOn<BootstrapCompleted>(async (evnt, context) =>
                {
                    await process.Start(evnt.Context).ConfigureAwait(false);
                })
                .ShutDownWith(async context =>
                {
                    await process.Stop(context).ConfigureAwait(false);
                });
        }
        
        public static AthenaBootstrapper UseProcess(this AthenaBootstrapper bootstrapper, 
            LongRunningProcess process, Func<Func<bool, AthenaContext, Task>, Task> subscribeToChanges)
        {
            return bootstrapper.UseProcess(new ConditionedProcessWrapper(process, subscribeToChanges));
        }
    }
}