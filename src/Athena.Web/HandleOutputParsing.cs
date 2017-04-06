using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Athena.Routing;

namespace Athena.Web
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class HandleOutputParsing
    {
        private readonly AppFunc _next;
        private readonly IReadOnlyCollection<ResultParser> _parsers;
        private readonly FindStatusCodeFromResult _findStatusCodeFromResult;

        public HandleOutputParsing(AppFunc next, IReadOnlyCollection<ResultParser> parsers,
            FindStatusCodeFromResult findStatusCodeFromResult)
        {
            _next = next;
            _parsers = parsers;
            _findStatusCodeFromResult = findStatusCodeFromResult;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var acceptedMediaTypes = environment.GetRequest().Headers.GetAcceptedMediaTypes().ToList();

            var parser = _parsers
                .Select(x => new
                {
                    Parser = x,
                    MatchResult = x
                        .MatchingMediaTypes
                        .SelectMany(y => acceptedMediaTypes.Select(z => new
                        {
                            MediaType = z,
                            IsMatch = z.Matches(y),
                            Priority = z.GetPriority()
                        }))
                        .Where(y => y.IsMatch)
                        .OrderBy(y => y.Priority)
                        .FirstOrDefault()
                })
                .Where(x => x.MatchResult != null)
                .OrderBy(x => x.MatchResult.Priority)
                .Select(x => x.Parser)
                .FirstOrDefault();

            if (parser == null)
            {
                environment.GetResponse().StatusCode = 406;

                return;
            }

            await _next(environment);

            var result = environment.Get<EndpointExecutionResult>("endpointresults");

            if (result == null)
                return;

            var response = environment.GetResponse();

            response.StatusCode = _findStatusCodeFromResult.FindFor(result.Result);

            if (result.Result == null)
                return;

            var outputResult = await parser.Parse(result.Result);

            response.Headers.ContentType = outputResult.ContentType;

            if (outputResult.Body == null)
                return;

            using (outputResult.Body)
                await response.Write(outputResult.Body).ConfigureAwait(false);
        }
    }
}