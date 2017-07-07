using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Athena.Configuration;

namespace Athena.Binding
{
    public class BindContext : EnvironmentDataBinder
    {
        public Task<DataBinderResult> Bind(Type to, IDictionary<string, object> environment)
        {
            return Task.FromResult(typeof(AthenaContext).GetTypeInfo().IsAssignableFrom(to)
                ? new DataBinderResult(environment.GetAthenaContext(), true)
                : new DataBinderResult(null, false));
        }
    }
}