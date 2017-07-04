using System;
using System.Collections.Generic;
using System.Linq;
using Athena.Authorization;
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
        private readonly ICollection<Authorizer> _authorizers = new List<Authorizer>();
        private readonly ICollection<MetaDataSupplier> _metaDataSuppliers = new List<MetaDataSupplier>();
        private readonly ICollection<FindCacheDataForRequest> _cacheDataFinders = new List<FindCacheDataForRequest>
        {
            new FindCacheDataForStaticFileRequest()
        };
        private IdentityFinder _identityFinder = new NullIdentityFinder();
        private Func<Type, IDictionary<string, object>, object> _createHandlerInstanceWith = (x, y) => 
            Activator.CreateInstance(x);

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

        public WebApplicationSettings CreateHandlerInstanceWith(
            Func<Type, IDictionary<string, object>, object> createInstance)
        {
            _createHandlerInstanceWith = createInstance;

            return this;
        }

        public WebApplicationSettings FindCacheConfigurationWith(FindCacheDataForRequest configFinder)
        {
            _cacheDataFinders.Add(configFinder);

            return this;
        }

        public WebApplicationSettings SupplyMetaDataWith(MetaDataSupplier supplier)
        {
            _metaDataSuppliers.Add(supplier);

            return this;
        }

        public WebApplicationSettings HandleTransactionsWith(Transaction transaction)
        {
            _transactions.Add(transaction);

            return this;
        }

        public WebApplicationSettings AuthorizeRequestsWith(Authorizer authorizer)
        {
            _authorizers.Add(authorizer);

            return this;
        }

        public WebApplicationSettings FindCurrentIdentityWith(IdentityFinder identityFinder)
        {
            _identityFinder = identityFinder;

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
                new UrlPatternRouter(routes, new DefaultRoutePatternMatcher(), _createHandlerInstanceWith),
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

            var authorizers = _authorizers.ToList();
            
            authorizers.Add(new MethodRouteConventionalAuthorizer(binders));

            return builder.First("HandleExceptions", next => new HandleExceptions(next,
                    async (exception, environment) =>
                    {
                        var context = environment.GetAthenaContext();

                        await context.Execute($"{Name}_error", environment).ConfigureAwait(false);
                    }).Invoke)
                .ContinueWith("MakeSureUrlIsUnique", next => new MakeSureUrlIsUnique(next).Invoke)
                .ContinueWith("HandleTransactions",
                    next => new HandleTransactions(next, _transactions.ToList()).Invoke,
                    () => _transactions.GetDiagnosticsData())
                .ContinueWith("SupplyMetaData", next => new SupplyMetaData(next, _metaDataSuppliers.ToList()).Invoke,
                    () => _metaDataSuppliers.GetDiagnosticsData())
                .ContinueWith("RouteToResource",
                    next => new RouteToResource(next, routers).Invoke, () => routes.GetDiagnosticsData())
                .ContinueWith("EnsureEndpointExists", next => new EnsureEndpointExists(next, routeCheckers,
                    async environment =>
                    {
                        var context = environment.GetAthenaContext();

                        await context.Execute($"{Name}_missing", environment).ConfigureAwait(false);
                    }).Invoke, () => routeCheckers.GetDiagnosticsData())
                .ContinueWith("UseCorrectOutputParser",
                    next => new UseCorrectOutputParser(next, mediaTypeFinders, outputParsers).Invoke,
                    () => outputParsers.GetDiagnosticsData())
                .ContinueWith("Authorize", next => new Authorize(next, authorizers, _identityFinder,
                    async environment =>
                    {
                        var context = environment.GetAthenaContext();

                        await context.Execute($"{Name}_unauthorized", environment).ConfigureAwait(false);
                    }).Invoke, () => authorizers.GetDiagnosticsData())
                .ContinueWith("ValidateParameters",
                    next => new ValidateParameters(next, _validators.ToList(), async environment =>
                    {
                        var context = environment.GetAthenaContext();

                        await context.Execute($"{Name}_invalid", environment).ConfigureAwait(false);
                    }).Invoke, () => _validators.GetDiagnosticsData())
                .ContinueWith("ValidateCache", next => new ValidateCache(next, _cacheDataFinders.ToList()).Invoke,
                    () => _cacheDataFinders.GetDiagnosticsData())
                .ContinueWith("ExecuteResource", next => new ExecuteResource(next, resourceExecutors).Invoke,
                    () => resourceExecutors.GetDiagnosticsData())
                .Last("WriteOutput",
                    next => new WriteOutput(next, new FindStatusCodeFromResultWithStatusCode()).Invoke);
        }
    }
}