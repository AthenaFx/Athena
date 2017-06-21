using System;
using System.Threading.Tasks;

namespace Athena.ApplicationTimeouts
{
    public static class Timeouts
    {
        public static TimeoutStore CurrentTimeoutStore { get; private set; }

        public static AthenaBootstrapper UsingTimeoutManager(this AthenaBootstrapper bootstrapper, TimeoutStore store)
        {
            CurrentTimeoutStore = store;

            return bootstrapper;
        }

        public static async Task RequestTimeout(object message, DateTime at)
        {
            if(CurrentTimeoutStore == null)
                throw new InvalidOperationException("No timeout store defined");

            await CurrentTimeoutStore.Add(new TimeoutData(Guid.NewGuid(), message, at));
        }
    }
}