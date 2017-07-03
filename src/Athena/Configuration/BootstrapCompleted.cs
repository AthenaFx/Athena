using System;

namespace Athena.Configuration
{
    public class BootstrapCompleted : SetupEvent
    {
        public BootstrapCompleted(string applicationName, string environment, TimeSpan executionTime)
        {
            ExecutionTime = executionTime;
            Environment = environment;
            ApplicationName = applicationName;
        }

        public string ApplicationName { get; }
        public string Environment { get; }
        public TimeSpan ExecutionTime { get; }
    }
}