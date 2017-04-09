using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            var result = environment.GetResourceResult();

            var response = environment.GetResponse();

            response.StatusCode = _findStatusCodeFromResult.FindFor(result);

            await environment.ParseAndWrite(result);

            await _next(environment).ConfigureAwait(false);
        }
    }
}