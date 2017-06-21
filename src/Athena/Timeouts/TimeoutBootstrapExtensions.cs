using System;
using System.Threading.Tasks;

namespace Athena.Timeouts
{
    public static class TimeoutBootstrapExtensions
    {
        public static TimeoutStore CurrentTimeoutStore { get; private set; }

        public static AthenaBootstrapper UsingTimeoutManager(this AthenaBootstrapper bootstrapper, TimeoutStore store)
        {
            CurrentTimeoutStore = store;

            return bootstrapper;
        }

        public static async Task RequestTimeout(this AthenaContext context, object message, DateTime at)
        {
            if(CurrentTimeoutStore == null)
                throw new InvalidOperationException("No timeout store defined");

            await CurrentTimeoutStore.Add(new TimeoutData(Guid.NewGuid(), message, at));
        }
    }
}