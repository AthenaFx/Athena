using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena.CommandHandling
{
    public class CommandDataBinder : EnvironmentDataBinder
    {
        public Task<DataBinderResult> Bind(Type to, IDictionary<string, object> environment)
        {
            return Task.FromResult(new DataBinderResult(environment["command"], true));
        }
    }
}