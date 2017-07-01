using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Athena.Binding;
using Athena.Configuration;
using Athena.MetaData;
using Athena.Resources;
using Athena.Routing;
using Athena.Transactions;

namespace Athena.CommandHandling
{
    public class CommandSenderConfiguration
    {
        private Func<AppFunctionBuilder, AppFunctionBuilder> _builder = builder =>
        {
            var routers = new List<EnvironmentRouter>
            {
                RouteCommandToMethod.New(x => x.DeclaringType.Name.EndsWith("Handler") 
                                              && x.Name == "Handle"
                                              && (x.ReturnType == typeof(void) || x.ReturnType == typeof(Task))
                                              && x.GetParameters().Length > 0, 
                    builder.Bootstrapper.ApplicationAssemblies)
            };

            var binders = new List<EnvironmentDataBinder>
            {
                new BindEnvironment(),
                new BindContext(),
                new CommandDataBinder()
            };

            var resourceExecutors = new List<ResourceExecutor>
            {
                new MethodResourceExecutor(binders)
            };
            
            return builder
                .Last("HandleTransactions",
                    next => new HandleTransactions(next, Enumerable.Empty<Transaction>()).Invoke)
                .Last("SupplyMetaData", next => new SupplyMetaData(next).Invoke)
                .Last("RouteToResource", next => new RouteToResource(next, routers, x =>
                    throw new CommandHandlerNotFoundException(x.Get<object>("command").GetType())).Invoke)
                .Last("ExecuteResource", next => new ExecuteResource(next, resourceExecutors).Invoke);
        };

        public CommandSenderConfiguration ConfigureApplication(Func<AppFunctionBuilder, AppFunctionBuilder> configure)
        {
            var currentBuilder = _builder;

            _builder = (builder => configure(currentBuilder(builder)));

            return this;
        }

        internal Func<AppFunctionBuilder, AppFunctionBuilder> GetBuilder()
        {
            return _builder;
        }
    }
}