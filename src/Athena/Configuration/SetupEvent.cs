using System;

namespace Athena.Configuration
{
    public interface SetupEvent
    {
        TimeSpan ExecutionTime { get; }
    }
}