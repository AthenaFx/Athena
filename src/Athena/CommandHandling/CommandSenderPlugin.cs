using System.Collections.Generic;
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
                                              && x.GetParameters().Length == 1)
            }.AsReadOnly();

            var binders = new List<EnvironmentDataBinder>
            {
                new CommandDataBinder()
            }.AsReadOnly();

            context.DefineApplication("commandhandler", AppFunctions
                .StartWith(next => new FindCorrectRoute(next, routers, x =>
                {
                    throw new CommandHandlerNotFoundException(x.Get<object>("command").GetType());
                }).Invoke)
                .Then(next => new ExecuteEndpoint(next, binders).Invoke)
                .Build());

            return Task.CompletedTask;
        }

        public Task ShutDown(AthenaContext context)
        {
            return Task.CompletedTask;
        }
    }
}