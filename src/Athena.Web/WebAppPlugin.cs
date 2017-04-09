using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Athena.Binding;
using Athena.Resources;
using Athena.Routing;
using Athena.Web.Caching;
using Athena.Web.ModelBinding;
using Athena.Web.Parsing;
using Athena.Web.Routing;

namespace Athena.Web
{
    public class WebAppPlugin : AthenaPlugin
    {
        public Task Start(AthenaContext context)
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

            var mediaTypeFinders = new ReadOnlyCollection<FindMediaTypesForRouterResult<RouterResult>>(
                new List<FindMediaTypesForRouterResult<RouterResult>>());

            context.DefineApplication("web", AppFunctions
                .StartWith(next => new HandleExceptions(next, (exception, environment) =>
                {
                    environment.GetResponse().StatusCode = 500;

                    return Task.CompletedTask;
                }).Invoke)
                .Then(next => new MakeSureUrlIsUnique(next).Invoke)
                .Then(next => new RouteToResource(next, routers, x => x.GetResponse().StatusCode = 404).Invoke)
                .Then(next => new ValidateMediaTypes(next, mediaTypeFinders, outputParsers).Invoke)
                .Then(next => new ValidateCache(next, routerCacheDataFinders).Invoke)
                .Then(next => new ExecuteResource(next, resourceExecutors).Invoke)
                .Then(next => new WriteOutput(next, new FindStatusCodeFromResultWithStatusCode()).Invoke)
                .Build());

            return Task.CompletedTask;
        }

        public Task ShutDown(AthenaContext context)
        {
            return Task.CompletedTask;
        }
    }
}