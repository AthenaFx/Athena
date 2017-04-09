using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Binding;
using Athena.Resources;
using Athena.Routing;

namespace Athena.CommandHandling
{
    public class CommandSenderPlugin : AthenaPlugin
    {
        public Task Start(AthenaContext context)
        {
            var routers = new List<EnvironmentRouter>
            {
                RouteCommandToMethod.New(x => x.Name == "Handle"
                                              && (x.ReturnType == typeof(void) || x.ReturnType == typeof(Task))
                                              && x.GetParameters().Length == 1)
            }.AsReadOnly();

            var binders = new List<EnvironmentDataBinder>
            {
                new CommandDataBinder()
            }.AsReadOnly();

            var resourceExecutors = new List<ResourceExecutor>
            {
                new MethodResourceExecutor(binders)
            };

            context.DefineApplication("commandhandler", AppFunctions
                .StartWith(next => new RouteToResource(next, routers, x => throw new CommandHandlerNotFoundException(x.Get<object>("command").GetType())).Invoke)
                .Then(next => new ExecuteResource(next, resourceExecutors).Invoke)
                .Build());

            return Task.CompletedTask;
        }

        public Task ShutDown(AthenaContext context)
        {
            return Task.CompletedTask;
        }
    }
}