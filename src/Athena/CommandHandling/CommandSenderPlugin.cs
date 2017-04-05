using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
                                              && x.GetParameters().Any())
            }.AsReadOnly();

            var executors = new List<EndpointExecutor>
            {
                new ExecuteMethodEndpoint(new List<EnvironmentDataBinder>
                {
                    new CommandDataBinder()
                })
            }.AsReadOnly();

            context.DefineApplication("commandhandler", AppFunctions
                .StartWith(next => new FindCorrectRoute(next, routers, x =>
                {
                    throw new CommandHandlerNotFoundException(x.Get<object>("command").GetType());
                }).Invoke)
                .Then(next => new ExecuteEndpoint(next, executors).Invoke)
                .Build());

            return Task.CompletedTask;
        }

        public Task ShutDown(AthenaContext context)
        {
            return Task.CompletedTask;
        }
    }
}