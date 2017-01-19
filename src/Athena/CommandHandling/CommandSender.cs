using System.Collections.Generic;
using System.Threading.Tasks;

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
    }
}