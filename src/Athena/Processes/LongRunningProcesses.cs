using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Athena.Configuration;

namespace Athena.Processes
{
    public static class LongRunningProcesses
    {
        private static readonly IDictionary<string, LongRunningProcess> Processes
            = new ConcurrentDictionary<string, LongRunningProcess>();

        public static AthenaBootstrapper UseProcess(this AthenaBootstrapper bootstrapper, LongRunningProcess process)
        {
            var id = Guid.NewGuid().ToString();

            Processes[id] = process;

            return bootstrapper
                .When<ContextCreated>()
                .Do(async (evnt, _) =>
                {
                    await process.Start(evnt.Context).ConfigureAwait(false);
                });
        }
        
        public static AthenaBootstrapper UseProcess(this AthenaBootstrapper bootstrapper, LongRunningProcess process, 
            Func<Func<bool, AthenaContext, Task>, Task> subscribeToChanges)
        {
            return bootstrapper.UseProcess(new ConditionedProcessWrapper(process, subscribeToChanges));
        }

        internal static Task StopAllProcesses()
        {
            return Task.WhenAll(Processes.Select(x => x.Value.Stop()));
        }
    }
}