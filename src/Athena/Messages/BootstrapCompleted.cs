using System;

namespace Athena.Messages
{
    public class BootstrapCompleted
    {
        public BootstrapCompleted(TimeSpan executionTime)
        {
            ExecutionTime = executionTime;
        }

        public TimeSpan ExecutionTime { get; }
    }
}