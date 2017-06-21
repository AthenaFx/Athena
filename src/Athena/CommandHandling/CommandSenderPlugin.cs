using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Binding;
using Athena.MetaData;
using Athena.Resources;
using Athena.Routing;
using Athena.Transactions;

namespace Athena.CommandHandling
{
    public class CommandSenderPlugin : AthenaPlugin
    {
        public Task Bootstrap(AthenaBootstrapper context)
        {
            var routers = new List<EnvironmentRouter>
            {
                RouteCommandToMethod.New(x => x.DeclaringType.Name.EndsWith("Handler") 
                                              && x.Name == "Handle"
                                              && (x.ReturnType == typeof(void) || x.ReturnType == typeof(Task))
                                              && x.GetParameters().Length > 0)
            }.AsReadOnly();

            var binders = new List<EnvironmentDataBinder>
            {
                new BindEnvironment(),
                new BindContext(),
                new CommandDataBinder()
            }.AsReadOnly();

            var resourceExecutors = new List<ResourceExecutor>
            {
                new MethodResourceExecutor(binders)
            };

            context.DefineApplication("commandhandler", AppFunctions
                .StartWith(next => new HandleTransactions(next).Invoke)
                .Then(next => new SupplyMetaData(next).Invoke)
                .Then(next => new RouteToResource(next, routers, x => 
                    throw new CommandHandlerNotFoundException(x.Get<object>("command").GetType())).Invoke)
                .Then(next => new ExecuteResource(next, resourceExecutors).Invoke)
                .Build(), false);

            return Task.CompletedTask;
        }

        public Task TearDown(AthenaBootstrapper context)
        {
            return Task.CompletedTask;
        }
    }
}