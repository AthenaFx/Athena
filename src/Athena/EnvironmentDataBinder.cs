using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena
{
    public interface EnvironmentDataBinder
    {
        Task<DataBinderResult> Bind(Type to, IDictionary<string, object> environment);
    }
}