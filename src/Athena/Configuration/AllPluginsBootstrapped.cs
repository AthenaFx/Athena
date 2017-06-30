using System;

namespace Athena.Configuration
{
    public class AllPluginsBootstrapped : SetupEvent
    {
        public AllPluginsBootstrapped(TimeSpan executionTime)
        {
            ExecutionTime = executionTime;
        }

        public TimeSpan ExecutionTime { get; }
    }
}