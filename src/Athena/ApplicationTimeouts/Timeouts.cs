using System;
using System.Threading.Tasks;
using Athena.Configuration;
using Athena.Logging;
using Athena.Processes;

namespace Athena.ApplicationTimeouts
{
    public static class Timeouts
    {
        public static TimeoutStore CurrentTimeoutStore { get; private set; }

        public static AthenaBootstrapper UseTimeouts(this AthenaBootstrapper bootstrapper, TimeoutStore store, 
            int secondsToSleepBetweenPolls = 10)
        {
            if(CurrentTimeoutStore != null)
                throw new InvalidOperationException("Can't use more then one timeout manager");
            
            Logger.Write(LogLevel.Debug, $"Registering timeout store ({store.GetType()})");
            
            CurrentTimeoutStore = store;

            return bootstrapper.UseProcess(new TimeoutManager(() => store, secondsToSleepBetweenPolls));
        }

        public static async Task RequestTimeout(object message, DateTime at)
        {
            if(CurrentTimeoutStore == null)
                throw new InvalidOperationException("No timeout store defined");

            Logger.Write(LogLevel.Debug, $"Requesting timeout at {at} ({message.GetType()})");
            
            await CurrentTimeoutStore.Add(new TimeoutData(Guid.NewGuid(), message, at));
        }
    }
}