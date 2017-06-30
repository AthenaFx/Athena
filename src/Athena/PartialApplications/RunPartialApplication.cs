using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Athena.PartialApplications
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    
    public class RunPartialApplication
    {
        private readonly AppFunc _next;
        private readonly Func<IDictionary<string, object>, string> _getApplication;
        
        public RunPartialApplication(AppFunc next, Func<IDictionary<string, object>, string> getApplication)
        {
            _next = next;
            _getApplication = getApplication;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var application = _getApplication(environment);

            if (!string.IsNullOrEmpty(application))
                await environment.GetAthenaContext().Execute(application, environment).ConfigureAwait(false);

            await _next(environment);
        }
    }
}