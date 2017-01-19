using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena.Web
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class SetStatusCode
    {
        private readonly AppFunc _next;
        private readonly int _statusCode;

        public SetStatusCode(AppFunc next, int statusCode)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            _next = next;
            _statusCode = statusCode;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            environment.GetResponse().StatusCode = _statusCode;

            await _next(environment).ConfigureAwait(false);
        }
    }
}