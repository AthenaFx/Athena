﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Athena.Routing;

namespace Athena.Web
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class SetCorrectStatusCode
    {
        private readonly AppFunc _next;

        public SetCorrectStatusCode(AppFunc next)
        {
            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            await _next(environment);

            var exception = environment.Get<Exception>("exception");

            if (exception != null)
            {
                environment.GetResponse().StatusCode = 500;

                return;
            }

            var outputResults = environment.Get("endpointresults", new List<EndpointExecutionResult>());

            if (!outputResults.Any(x => x.Success))
            {
                environment.GetResponse().StatusCode = 404;

                return;
            }
        }
    }
}