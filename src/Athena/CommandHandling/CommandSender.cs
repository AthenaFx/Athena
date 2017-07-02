using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Configuration;

namespace Athena.CommandHandling
{
    public static class CommandSender
    {
        public static Task SendCommand<TCommand>(this AthenaContext context, TCommand command)
        {
            return context.Execute("commandhandler", new Dictionary<string, object>
            {
                ["command"] = command
            });
        }

        public static Task SendCommand<TCommand>(this IDictionary<string, object> environment, TCommand command)
        {
            var context = environment.GetAthenaContext();

            return context.SendCommand(command);
        }

        public static PartConfiguration<CommandSenderConfiguration> EnableCommandSender(
            this AthenaBootstrapper bootstrapper)
        {
            return bootstrapper.ConfigureWith<CommandSenderConfiguration>((conf, context) =>
            {
                context.DefineApplication(conf.Name, conf.GetApplicationBuilder());
                
                return Task.CompletedTask;
            });
        }
    }
}