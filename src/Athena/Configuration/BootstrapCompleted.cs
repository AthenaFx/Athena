using System;

namespace Athena.Configuration
{
    public class BootstrapCompleted : SetupEvent
    {
        public BootstrapCompleted(string applicationName, string environment, TimeSpan executionTime, 
            TimeSpan locatedComponentsIn, TimeSpan startupsRanIn, TimeSpan setupRanIn)
        {
            ExecutionTime = executionTime;
            LocatedComponentsIn = locatedComponentsIn;
            StartupsRanIn = startupsRanIn;
            SetupRanIn = setupRanIn;
            Environment = environment;
            ApplicationName = applicationName;
        }

        public string ApplicationName { get; }
        public string Environment { get; }
        public TimeSpan ExecutionTime { get; }
        public TimeSpan LocatedComponentsIn { get; }
        public TimeSpan StartupsRanIn { get; }
        public TimeSpan SetupRanIn { get; }
    }
}