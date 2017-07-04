using System.Collections.Generic;

namespace Athena.Authorization
{
    public class AuthenticationIdentity
    {
        public AuthenticationIdentity(string name, IReadOnlyDictionary<string, object> data)
        {
            Name = name;
            Data = data;
        }

        public string Name { get; }
        public IReadOnlyDictionary<string, object> Data { get; }
    }
}