using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena.Binding
{
    public class BindSettings : EnvironmentDataBinder
    {
        public Task<DataBinderResult> Bind(Type to, IDictionary<string, object> environment)
        {
            var setting = environment.GetAthenaContext().GetSetting(to);

            return Task.FromResult(setting != null
                ? new DataBinderResult(setting, true)
                : new DataBinderResult(null, false));
        }
    }
}