using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Athena.Routing;

namespace Athena.Web
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class WriteWebOutput
    {
        private readonly AppFunc _next;
        private readonly IReadOnlyCollection<ResultParser> _parsers;

        public WriteWebOutput(AppFunc next, IReadOnlyCollection<ResultParser> parsers)
        {
            _next = next;
            _parsers = parsers;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var outputResults = environment.Get("endpointresults", new List<EndpointExecutionResult>());
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
                outputResults.Clear();

                return;
            }

            foreach (var result in outputResults.Where(x => x.Success && x.Result != null))
            {
                var outputResult = await parser.Parse(result.Result);

                var response = environment.GetResponse();

                response.Headers.ContentType = outputResult.ContentType;

                if (outputResult.Body == null) continue;

                using (outputResult.Body)
                    await response.Write(outputResult.Body).ConfigureAwait(false);
            }
        }
    }
}