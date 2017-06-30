using System;

namespace Athena.Configuration
{
    public class ContextCreated : SetupEvent
    {
        public ContextCreated(TimeSpan executionTime, AthenaContext context)
        {
            ExecutionTime = executionTime;
            Context = context;
        }

        public TimeSpan ExecutionTime { get; }
        public AthenaContext Context { get; }
    }
}