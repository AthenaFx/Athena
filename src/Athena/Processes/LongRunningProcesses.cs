using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

            return bootstrapper;
        }

        internal static Task StartAllProcesses()
        {
            return Task.WhenAll(Processes.Select(x => x.Value.Start()));
        }

        internal static Task StopAllProcesses()
        {
            return Task.WhenAll(Processes.Select(x => x.Value.Stop()));
        }
    }
}