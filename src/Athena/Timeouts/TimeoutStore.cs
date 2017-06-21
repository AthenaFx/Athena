using System;
using System.Threading.Tasks;

namespace Athena.Timeouts
{
    public interface TimeoutStore
    {
        Task<DateTime> GetNextChunk(DateTime startSlice, Func<Tuple<TimeoutData, DateTime>, Task> timeoutFound);
        Task Add(TimeoutData timeout);
    }
}