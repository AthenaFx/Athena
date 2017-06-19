using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Binding;
using Athena.MetaData;
using Athena.Resources;
using Athena.Routing;
using Athena.Transactions;
using Athena.Web.Caching;
using Athena.Web.ModelBinding;
using Athena.Web.Parsing;
using Athena.Web.Routing;
using Athena.Web.Validation;

namespace Athena.Web
{
    public class WebAppPlugin : AthenaPlugin
    {
        public Task Bootstrap(AthenaBootstrapper context)
        {
            var routes = RestfulEndpointConventions.BuildRoutes();

            var fileHandlers = new List<StaticFileReader>
            {
                new ReadStaticFilesFromFileSystem("index.html", "index.htm")
            };

            var routers = new List<EnvironmentRouter>
            {
                new UrlPatternRouter(routes, new DefaultRoutePatternMatcher()),
                new StaticFileRouter(fileHandlers)
            };

            var binders = new List<EnvironmentDataBinder>
            {
                new BindEnvironment(),
                new WebDataBinder(ModelBinders.GetAll())
            };

            var outputParsers = new List<ResultParser>
            {
                new ParseOutputAsJson()
            };

            var resourceExecutors = new List<ResourceExecutor>
            {
                new MethodResourceExecutor(binders)
            };

            var routerCacheDataFinders = new List<FindCacheDataForRoute>
            {
                new FindCacheDataForStaticFileRoute()
            };

            var mediaTypeFinders = new List<FindMediaTypesForRouterResult>
            {
                new FindAvailableMediaTypesFromMethodRouteResult(binders),
                new FindAvailableMediaTypesFromStaticFileRouteResult()
            };

            var routeCheckers = new List<CheckIfResourceExists>
            {
                new CheckIfMethodResourceExists(binders)
            };

            context.DefineApplication("web", AppFunctions
                .StartWith(next => new HandleExceptions(next, (exception, environment) =>
                {
                    environment.GetResponse().StatusCode = 500;

                    return Task.CompletedTask;
                }).Invoke)
                .Then(next => new MakeSureUrlIsUnique(next).Invoke)
                .Then(next => new HandleTransactions(next).Invoke)
                .Then(next => new SupplyMetaData(next).Invoke)
                .Then(next => new RouteToResource(next, routers, x => x.GetResponse().StatusCode = 404).Invoke)
                .Then(next => new EnsureEndpointExists(next, routeCheckers).Invoke)
                .Then(next => new UseCorrectOutputParser(next, mediaTypeFinders, outputParsers).Invoke)
                .Then(next => new ValidateParameters(next, new List<ValidateRouteResult>()).Invoke)
                .Then(next => new ValidateCache(next, routerCacheDataFinders).Invoke)
                .Then(next => new ExecuteResource(next, resourceExecutors).Invoke)
                .Then(next => new WriteOutput(next, new FindStatusCodeFromResultWithStatusCode()).Invoke)
                .Build(), false);

            return Task.CompletedTask;
        }

        public Task TearDown(AthenaBootstrapper context)
        {
            return Task.CompletedTask;
        }
    }
}