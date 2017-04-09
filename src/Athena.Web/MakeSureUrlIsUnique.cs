using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena.Web
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class MakeSureUrlIsUnique
    {
        private readonly AppFunc _next;

        public MakeSureUrlIsUnique(AppFunc next)
        {
            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var request = environment.GetRequest();

            if (!request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                await _next(environment).ConfigureAwait(false);

                return;
            }

            var uri = request.Uri;

            var validSegmentPart = uri.LocalPath.ToLower();

            if (validSegmentPart.EndsWith("/") && validSegmentPart.Length > 1)
                validSegmentPart = validSegmentPart.Substring(0, validSegmentPart.Length - 1);

            if (validSegmentPart == uri.LocalPath)
            {
                await _next(environment).ConfigureAwait(false);

                return;
            }

            var redirectTo = validSegmentPart;

            if (!string.IsNullOrEmpty(uri.Query))
                redirectTo = $"{redirectTo}{uri.Query}";

            var response = environment.GetResponse();

            response.StatusCode = 301;
            response.Headers.SetHeader("Location", redirectTo);
        }
    }
}