﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Routing;
using Athena.Web.ModelBinding;
using Athena.Web.Routing;

namespace Athena.Web
{
    public class WebAppPlugin : AthenaPlugin
    {
        public Task Start(AthenaContext context)
        {
            var routes = RestfulEndpointConventions.BuildRoutes();

            var routers = new List<EnvironmentRouter>
            {
                new UrlPatternRouter(routes, new DefaultRoutePatternMatcher())
            }.AsReadOnly();

            var binders = new List<EnvironmentDataBinder>
            {
                new WebDataBinder(ModelBinders.GetAll())
            }.AsReadOnly();

            var outputParsers = new List<ResultParser>
            {
                new ParseOutputAsJson()
            };

            context.DefineApplication("web", AppFunctions
                .StartWith(next => new MakeSureUniqueUrl(next).Invoke)
                .Then(next => new GzipOutput(next).Invoke)
                .Then(next => new HandleExceptions(next, (exception, environment) =>
                {
                    environment.GetResponse().StatusCode = 500;

                    return Task.CompletedTask;
                }).Invoke)
                .Then(next => new FindCorrectRoute(next, routers, x => x.GetResponse().StatusCode = 404).Invoke)
                .Then(next => new HandleOutputParsing(next, outputParsers, new FindStatusCodeFromResultWithStatusCode()).Invoke)
                .Then(next => new ExecuteEndpoint(next, binders, (validationResult, environment) =>
                    {
                        environment.GetResponse().StatusCode = validationResult.ValidationStatus ?? 422;

                        return Task.CompletedTask;
                    }
                ).Invoke)
                .Build());

            return Task.CompletedTask;
        }

        public Task ShutDown(AthenaContext context)
        {
            return Task.CompletedTask;
        }
    }
}