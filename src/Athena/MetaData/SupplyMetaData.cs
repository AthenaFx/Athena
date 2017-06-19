using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena.MetaData
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    
    public class SupplyMetaData
    {
        private readonly AppFunc _next;

        public SupplyMetaData(AppFunc next)
        {
            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            using (MetaDataManagement.GatherMetaData(environment))
            {
                await _next(environment).ConfigureAwait(false);
            }
        }
    }
}