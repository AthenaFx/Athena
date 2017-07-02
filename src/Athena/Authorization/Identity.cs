using System.Collections.Generic;

namespace Athena.Authorization
{
    public class Identity
    {
        public Identity(string name, IReadOnlyDictionary<string, string> data)
        {
            Name = name;
            Data = data;
        }

        public string Name { get; }
        public IReadOnlyDictionary<string, string> Data { get; }
    }
}