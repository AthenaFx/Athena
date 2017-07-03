using System;
using System.Threading.Tasks;

namespace Athena.ApplicationTimeouts
{
    public class NullTimeoutStore : TimeoutStore
    {
        public Task<DateTime> GetNextChunk(DateTime startSlice, Func<Tuple<TimeoutData, DateTime>, Task> timeoutFound)
        {
            return Task.FromResult(DateTime.UtcNow.AddHours(1));
        }

        public Task Add(TimeoutData timeout)
        {
            return Task.CompletedTask;
        }
    }
}