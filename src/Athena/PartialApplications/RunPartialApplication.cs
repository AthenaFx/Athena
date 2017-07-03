using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Logging;

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
            else
            {
                Logger.Write(LogLevel.Info,
                    $"Can't find any applications to execute for request {environment.GetRequestId()}");
            }

            await _next(environment);
        }
    }
}