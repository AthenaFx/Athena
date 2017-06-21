using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Athena.Binding
{
    public class BindContext : EnvironmentDataBinder
    {
        public Task<DataBinderResult> Bind(Type to, IDictionary<string, object> environment)
        {
            return Task.FromResult(typeof(AthenaContext).IsAssignableFrom(to)
                ? new DataBinderResult(environment.Get<AthenaContext>("context"), true)
                : new DataBinderResult(null, false));
        }
    }
}