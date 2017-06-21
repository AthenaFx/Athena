using System;
using System.Threading.Tasks;
using Athena.Processes;

namespace Athena.ApplicationTimeouts
{
    public class TimeoutPlugin : AthenaPlugin
    {
        public Task Bootstrap(AthenaBootstrapper context)
        {
            //TODO:Add config for waiting period
            context.UseProcess(new TimeoutManager(() => Timeouts.CurrentTimeoutStore, 10));
        
            return Task.CompletedTask;
        }

        public Task TearDown(AthenaBootstrapper context)
        {
            throw new NotImplementedException();
        }
    }
}