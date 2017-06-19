using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Athena.Binding
{
    public class BindEnvironment : EnvironmentDataBinder
    {
        public Task<DataBinderResult> Bind(Type to, IDictionary<string, object> environment)
        {
            return Task.FromResult(typeof(IDictionary<string, object>).IsAssignableFrom(to) 
                ? new DataBinderResult(environment, true) 
                : new DataBinderResult(null, false));
        }
    }
}