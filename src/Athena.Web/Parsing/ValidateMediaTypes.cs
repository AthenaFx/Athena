using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Athena.Routing;

namespace Athena.Web.Parsing
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ValidateMediaTypes
    {
        private readonly AppFunc _next;

        private readonly IReadOnlyCollection<FindMediaTypesForRouterResult<RouterResult>>
            _findMediaTypesForRouterResults;

        private readonly IReadOnlyCollection<ResultParser> _parsers;

        public ValidateMediaTypes(AppFunc next,
            IReadOnlyCollection<FindMediaTypesForRouterResult<RouterResult>> findMediaTypesForRouterResults,
            IReadOnlyCollection<ResultParser> parsers)
        {
            _next = next;
            _findMediaTypesForRouterResults = findMediaTypesForRouterResults;
            _parsers = parsers;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var routerResult = environment.GetRouteResult();

            if (routerResult == null)
            {
                await _next(environment).ConfigureAwait(false);

                return;
            }

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

            var isValid = (bool) GetType()
                .GetMethod("CanRender")
                .MakeGenericMethod(routerResult.GetType())
                .Invoke(this, new object[] {routerResult, acceptedMediaTypes});

            if (parser == null || !isValid)
            {
                environment.GetResponse().StatusCode = 406;

                return;
            }

            using (environment.UsingParser(parser))
            {
                await _next(environment);
            }
        }

        protected bool CanRender<TRouterResult>(TRouterResult routerResult,
            IReadOnlyCollection<AcceptedMediaType> acceptedMediaTypes) where TRouterResult : RouterResult
        {
            var finder = _findMediaTypesForRouterResults
                .OfType<FindMediaTypesForRouterResult<TRouterResult>>()
                .FirstOrDefault();

            if (finder == null)
                return true;

            var availableMediaTypes = finder.FindAvailableFor(routerResult);

            return acceptedMediaTypes.Any(x => availableMediaTypes.Any(x.Matches));
        }
    }
}