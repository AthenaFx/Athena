using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Athena.Binding;
using Athena.Configuration;
using Athena.Diagnostics;
using Athena.Resources;
using Athena.Routing;
using Athena.Transactions;
using Athena.Web.Caching;
using Athena.Web.ModelBinding;
using Athena.Web.Parsing;
using Athena.Web.Routing;
using Athena.Web.Validation;

namespace Athena.Web.Diagnostics
{   
    public static class WebDiagnostics
    {
        public static string BaseUrl { get; private set; }
        
        public static PartConfiguration<WebApplicationsSettings> WithUiAt(
            this PartConfiguration<DiagnosticsConfiguration> config, string baseUrl)
        {
            BaseUrl = baseUrl;
            
            var routes = DefaultRouteConventions.BuildRoutes(x => $"{baseUrl}/{x}", 
                x => x.Namespace == "Athena.Web.Diagnostics.Endpoints.Home",
                new List<string>
                {
                    "Step"
                }, typeof(WebDiagnostics).GetTypeInfo().Assembly);

            var routers = new List<EnvironmentRouter>
            {
                new UrlPatternRouter(routes, new DefaultRoutePatternMatcher())
            };

            var binders = new List<EnvironmentDataBinder>
            {
                new BindEnvironment(),
                new BindContext(),
                new WebDataBinder(ModelBinders.GetAll())
            };

            var outputParsers = new List<ResultParser>
            {
                new ParseOutputAsJson(),
                new ParseOutputAsHtml()
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

            return config
                .UsingWeb()
                .AddApplication("diagnostics_web", builder => builder
                        .Last("HandleExceptions", next => new HandleExceptions(next, async (exception, environment) =>
                        {
                            var response = environment.GetResponse();
                            response.StatusCode = 500;

                            var currentException = exception;

                            var responseText = new StringBuilder();

                            while (currentException != null)
                            {
                                responseText.Append(currentException.Message);
                                responseText.Append(currentException.StackTrace);

                                currentException = currentException.InnerException;
                            }

                            await response.Write(responseText.ToString());
                        }).Invoke)
                        .Last("MakeSureUrlIsUnique", next => new MakeSureUrlIsUnique(next).Invoke)
                        .Last("HandleTransactions",
                            next => new HandleTransactions(next, Enumerable.Empty<Transaction>()).Invoke)
                        .Last("RouteToResource",
                            next => new RouteToResource(next, routers, x => x.GetResponse().StatusCode = 404).Invoke)
                        .Last("EnsureEndpointExists", next => new EnsureEndpointExists(next, routeCheckers).Invoke)
                        .Last("UseCorrectOutputParser",
                            next => new UseCorrectOutputParser(next, mediaTypeFinders, outputParsers).Invoke)
                        .Last("ValidateParameters",
                            next => new ValidateParameters(next, new List<ValidateRouteResult>()).Invoke)
                        .Last("ValidateCache", next => new ValidateCache(next, routerCacheDataFinders).Invoke)
                        .Last("ExecuteResource", next => new ExecuteResource(next, resourceExecutors).Invoke)
                        .Last("WriteOutput",
                            next => new WriteOutput(next, new FindStatusCodeFromResultWithStatusCode()).Invoke),
                    environment => environment.GetRequest().Uri.LocalPath.StartsWith($"/{baseUrl}"));
        }
    }
}