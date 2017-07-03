using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Binding;
using Athena.Logging;

namespace Athena.CommandHandling
{
    public class CommandDataBinder : EnvironmentDataBinder
    {
        public Task<DataBinderResult> Bind(Type to, IDictionary<string, object> environment)
        {
            Logger.Write(LogLevel.Debug, $"Binding command to {to}");
            
            return Task.FromResult(new DataBinderResult(environment["command"], true));
        }
    }
}