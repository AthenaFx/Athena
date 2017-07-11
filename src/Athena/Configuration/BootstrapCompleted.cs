using System;
using System.Collections.Generic;

namespace Athena.Configuration
{
    public class BootstrapCompleted : SetupEvent
    {
        public BootstrapCompleted(string applicationName, string environment, 
            IReadOnlyDictionary<string, TimeSpan> timings)
        {
            Environment = environment;
            Timings = timings;
            ApplicationName = applicationName;
        }

        public string ApplicationName { get; }
        public string Environment { get; }
        public IReadOnlyDictionary<string, TimeSpan> Timings { get; }
    }
}