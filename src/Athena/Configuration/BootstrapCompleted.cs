﻿using System;

namespace Athena.Configuration
{
    public class BootstrapCompleted : SetupEvent
    {
        public BootstrapCompleted(string applicationName, string environment, TimeSpan executionTime, 
            TimeSpan locatedComponentsIn)
        {
            ExecutionTime = executionTime;
            LocatedComponentsIn = locatedComponentsIn;
            Environment = environment;
            ApplicationName = applicationName;
        }

        public string ApplicationName { get; }
        public string Environment { get; }
        public TimeSpan ExecutionTime { get; }
        public TimeSpan LocatedComponentsIn { get; }
    }
}