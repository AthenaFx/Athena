﻿namespace Athena.Configuration
{
    public class ShutdownStarted
    {
        public ShutdownStarted(string applicationName, string environment)
        {
            ApplicationName = applicationName;
            Environment = environment;
        }

        public string ApplicationName { get; }
        public string Environment { get; }
    }
}