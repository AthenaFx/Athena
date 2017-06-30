using System;

namespace Athena.Configuration
{
    public class PluginBootstrapped : SetupEvent
    {
        public PluginBootstrapped(TimeSpan executionTime, Type pluginType)
        {
            ExecutionTime = executionTime;
            PluginType = pluginType;
        }

        public TimeSpan ExecutionTime { get; }
        public Type PluginType { get; }
    }
}