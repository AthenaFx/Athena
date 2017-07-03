using System;
using System.Collections.Generic;

namespace Athena.Configuration
{
    public class ApplicationCompiled : SetupEvent
    {
        public ApplicationCompiled(string name, IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> data, 
            TimeSpan duration)
        {
            Name = name;
            Data = data;
            Duration = duration;
        }

        public string Name { get; }
        public TimeSpan Duration { get; }
        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> Data { get; }
    }
}