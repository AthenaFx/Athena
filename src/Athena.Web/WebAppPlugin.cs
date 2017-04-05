﻿using System;
using System.Collections.Generic;
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

            var executors = new List<EndpointExecutor>
            {
                new ExecuteMethodEndpoint(new List<EnvironmentDataBinder>
                {
                    new WebDataBinder(ModelBinders.GetAll())
                })
            }.AsReadOnly();

            var outputParsers = new List<Tuple<Func<IDictionary<string, object>, bool>, ResultParser>>
            {
                new Tuple<Func<IDictionary<string, object>, bool>, ResultParser>(ParseOutputAsJson.Matches, new ParseOutputAsJson())
            };

            context.DefineApplication("web", AppFunctions
                .StartWith(next => new GzipOutput(next).Invoke)
                .Then(next => new HandleExceptions(next).Invoke)
                .Then(next => new FindCorrectRoute(next, routers).Invoke)
                .Then(next => new ExecuteEndpoint(next, executors).Invoke)
                .Then(next => new WriteWebOutput(next, outputParsers).Invoke)
                .Build());

            return Task.CompletedTask;
        }

        public Task ShutDown(AthenaContext context)
        {
            return Task.CompletedTask;
        }
    }
}