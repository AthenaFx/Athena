using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Configuration;
using Athena.Logging;

namespace Athena.CommandHandling
{
    public static class CommandSender
    {
        public static Task SendCommand<TCommand>(this AthenaContext context, TCommand command)
        {
            Logger.Write(LogLevel.Debug, $"Sending command {typeof(TCommand)}");
            
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
            Logger.Write(LogLevel.Debug, "Enabling command sender");

            return bootstrapper.Part<CommandSenderConfiguration>()
                .OnSetup((conf, context) =>
                {
                    Logger.Write(LogLevel.Debug, $"Configuring command sender");

                    context.DefineApplication(conf.Name, conf.GetApplicationBuilder());

                    return Task.CompletedTask;
                });
        }
    }
}