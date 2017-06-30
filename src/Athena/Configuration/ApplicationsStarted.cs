using System;

namespace Athena.Configuration
{
    public class ApplicationsStarted : SetupEvent
    {
        public ApplicationsStarted(string applicationName, string environment, TimeSpan executionTime)
        {
            ExecutionTime = executionTime;
            ApplicationName = applicationName;
            Environment = environment;
        }

        public string ApplicationName { get; }
        public string Environment { get; }
        public TimeSpan ExecutionTime { get; }
    }
}