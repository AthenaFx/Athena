using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Athena.Logging;
using Athena.Resources;

namespace Athena.Web.Parsing
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class WriteOutput
    {
        private readonly AppFunc _next;
        private readonly FindStatusCodeFromResult _findStatusCodeFromResult;

        public WriteOutput(AppFunc next, FindStatusCodeFromResult findStatusCodeFromResult)
        {
            _next = next;
            _findStatusCodeFromResult = findStatusCodeFromResult;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            Logger.Write(LogLevel.Debug, $"Starting output writing for request {environment.GetRequestId()}");
            
            var result = environment.GetResourceResult();

            var response = environment.GetResponse();

            response.StatusCode = _findStatusCodeFromResult.FindFor(result);

            await environment.ParseAndWrite(result);

            Logger.Write(LogLevel.Debug, $"Finished writing output for request {environment.GetRequestId()}");
            
            await _next(environment).ConfigureAwait(false);
        }
    }
}