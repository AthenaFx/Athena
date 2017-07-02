using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Athena.Web.Parsing
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class UseCorrectOutputParser
    {
        private readonly AppFunc _next;

        private readonly IReadOnlyCollection<FindMediaTypesForRequest>
            _findMediaTypesForRouterResults;

        private readonly IReadOnlyCollection<ResultParser> _parsers;

        public UseCorrectOutputParser(AppFunc next,
            IReadOnlyCollection<FindMediaTypesForRequest> findMediaTypesForRouterResults,
            IReadOnlyCollection<ResultParser> parsers)
        {
            _next = next;
            _findMediaTypesForRouterResults = findMediaTypesForRouterResults;
            _parsers = parsers;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var acceptedMediaTypes = environment.GetRequest().Headers.GetAcceptedMediaTypes().ToList();
            var renderableMediaTypes = await FindRenderableMediaTypes(environment).ConfigureAwait(false);

            var parser = _parsers
                .Select(x => new
                {
                    Parser = x,
                    MatchResult = x
                        .MatchingMediaTypes
                        .SelectMany(y => acceptedMediaTypes.Select(z => new
                        {
                            MediaType = z,
                            IsMatch = z.Matches(y) && 
                                      (!renderableMediaTypes.Any() || renderableMediaTypes.Any(z.Matches)),
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

            using (environment.UsingParser(parser))
            {
                await _next(environment);
            }
        }

        protected async Task<IReadOnlyCollection<string>> FindRenderableMediaTypes(
            IDictionary<string, object> environment)
        {
            var renderable = new List<string>();

            foreach (var findMediaTypesForRouterResult in _findMediaTypesForRouterResults)
            {
                renderable.AddRange(await findMediaTypesForRouterResult.FindAvailableFor(environment)
                    .ConfigureAwait(false));
            }

            return renderable;
        }
    }
}