using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Athena.Binding;
using Athena.Configuration;
using Athena.MetaData;
using Athena.PartialApplications;
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
    public static class WebBootstrapExtensions
    {
        private static bool _hasConfiguredWeb;
        
        public static PartConfiguration<WebApplicationsSettings> UsingWeb(this AthenaBootstrapper bootstrapper)
        {
            if (_hasConfiguredWeb)
                return bootstrapper.Configure<WebApplicationsSettings>();
            
            _hasConfiguredWeb = true;
                
            return bootstrapper
                .ConfigureWith<WebApplicationsSettings>((conf, context) =>
                {
                    var applications = conf.GetApplications();

                    foreach (var application in applications)
                        context.DefineApplication(application.Key, application.Value.Item3);
                        
                    context.DefineApplication("web", builder => builder
                        .First("RunPartialApplication", next => new RunPartialApplication(next, env =>
                        {
                            return applications
                                .OrderBy(x => x.Value.Item1)
                                .Where(x => x.Value.Item2(env))
                                .Select(x => x.Key)
                                .FirstOrDefault();
                        }).Invoke));
                        
                    return Task.CompletedTask;
                });
        }

        public static PartConfiguration<WebApplicationsSettings> UsingDefaultWeb(this AthenaBootstrapper bootstrapper)
        {
            var routes = DefaultRouteConventions
                .BuildRoutes(bootstrapper.ApplicationAssemblies.ToArray());

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

            return bootstrapper
                .UsingWeb()
                .AddApplication("default_web", builder => builder
                    .Last("HandleExceptions", next => new HandleExceptions(next, async (exception, environment) =>
                    {
                        var response = environment.GetResponse();
                        response.StatusCode = 500;

                        var currentException = exception;

                        while (currentException != null)
                        {
                            await response.Write(currentException.Message);
                            await response.Write(currentException.StackTrace);

                            currentException = currentException.InnerException;
                        }
                    }).Invoke)
                    .Last("MakeSureUrlIsUnique", next => new MakeSureUrlIsUnique(next).Invoke)
                    .Last("HandleTransactions",
                        next => new HandleTransactions(next, Enumerable.Empty<Transaction>()).Invoke)
                    .Last("SupplyMetaData", next => new SupplyMetaData(next).Invoke)
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
                        next => new WriteOutput(next, new FindStatusCodeFromResultWithStatusCode()).Invoke));
        }
        
        public static PartConfiguration<WebApplicationsSettings> ConfigureDefaultApplication(
            this PartConfiguration<WebApplicationsSettings> config,
            Func<AppFunctionBuilder, AppFunctionBuilder> defaultBuilder)
        {
            return config.UpdateSettings(x => x.ConfigureApplication("default_web", defaultBuilder));
        }

        public static PartConfiguration<WebApplicationsSettings> AddApplication(
            this PartConfiguration<WebApplicationsSettings> config, string name,
            Func<AppFunctionBuilder, AppFunctionBuilder> defaultBuilder,
            Func<IDictionary<string, object>, bool> filter = null)
        {
            return config.UpdateSettings(x => x.AddApplication(name, defaultBuilder, filter));
        }
        
        public static PartConfiguration<WebApplicationsSettings> ConfigureApplication(
            this PartConfiguration<WebApplicationsSettings> config, string name,
            Func<AppFunctionBuilder, AppFunctionBuilder> defaultBuilder,
            Func<IDictionary<string, object>, bool> filter = null)
        {
            return config.UpdateSettings(x => x.ConfigureApplication(name, defaultBuilder, filter));
        }
    }
}