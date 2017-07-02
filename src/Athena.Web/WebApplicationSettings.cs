using System;
using System.Collections.Generic;
using System.Linq;
using Athena.Binding;
using Athena.Configuration;
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
    public class WebApplicationSettings : AppFunctionDefinition
    {
        private readonly ICollection<Transaction> _transactions = new List<Transaction>();
        private readonly ICollection<ValidateRouteResult> _validators = new List<ValidateRouteResult>();

        private Func<WebApplicationSettings, AthenaBootstrapper, IReadOnlyCollection<Route>> _buildRoutes
            = (settings, bootstrapper) => DefaultRouteConventions
                .BuildRoutes(x => string.IsNullOrEmpty(settings.BaseUrl) ? x : $"{settings.BaseUrl}/{x}",
                    bootstrapper.ApplicationAssemblies.ToArray());

        public string Name { get; private set; }

        public string BaseUrl { get; private set; }
        
        internal WebApplicationSettings WithName(string name)
        {
            Name = name;

            return this;
        }

        public WebApplicationSettings HandleTransactionsWith(Transaction transaction)
        {
            _transactions.Add(transaction);

            return this;
        }

        public WebApplicationSettings ValidateWith(ValidateRouteResult validator)
        {
            _validators.Add(validator);

            return this;
        }

        public WebApplicationSettings WithBaseUrl(string baseUrl)
        {
            BaseUrl = baseUrl;

            return this;
        }

        public WebApplicationSettings BuildRoutesWith(
            Func<WebApplicationSettings, AthenaBootstrapper, IReadOnlyCollection<Route>> builder)
        {
            _buildRoutes = builder;

            return this;
        }
        
        protected override AppFunctionBuilder DefineDefaultApplication(AppFunctionBuilder builder)
        {
            var routes = _buildRoutes(this, builder.Bootstrapper);

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

            var mediaTypeFinders = new List<FindMediaTypesForRequest>
            {
                new FindAvailableMediaTypesFromMethodRouteResult(binders),
                new FindAvailableMediaTypesFromStaticFileRouteResult()
            };

            var routeCheckers = new List<CheckIfResourceExists>
            {
                new CheckIfRouteExists(),
                new CheckIfMethodResourceExists(binders)
            };

            return builder.Last("HandleExceptions", next => new HandleExceptions(next, async (exception, environment) =>
                {
                    var context = environment.GetAthenaContext();

                    await context.Execute($"{Name}_error", environment).ConfigureAwait(false);
                }).Invoke)
                .Last("MakeSureUrlIsUnique", next => new MakeSureUrlIsUnique(next).Invoke)
                .Last("HandleTransactions",
                    next => new HandleTransactions(next, _transactions.ToList()).Invoke)
                .Last("SupplyMetaData", next => new SupplyMetaData(next).Invoke)
                .Last("RouteToResource",
                    next => new RouteToResource(next, routers).Invoke)
                .Last("EnsureEndpointExists", next => new EnsureEndpointExists(next, routeCheckers, async environment => 
                {
                    var context = environment.GetAthenaContext();

                    await context.Execute($"{Name}_missing", environment).ConfigureAwait(false);
                }).Invoke)
                .Last("UseCorrectOutputParser",
                    next => new UseCorrectOutputParser(next, mediaTypeFinders, outputParsers).Invoke)
                .Last("ValidateParameters",
                    next => new ValidateParameters(next, _validators.ToList()).Invoke)
                .Last("ValidateCache", next => new ValidateCache(next, routerCacheDataFinders).Invoke)
                .Last("ExecuteResource", next => new ExecuteResource(next, resourceExecutors).Invoke)
                .Last("WriteOutput",
                    next => new WriteOutput(next, new FindStatusCodeFromResultWithStatusCode()).Invoke);
        }
    }
}