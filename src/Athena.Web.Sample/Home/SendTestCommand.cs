using System.Threading.Tasks;
using Athena.CommandHandling;
using Athena.Web.Sample.Commands;

namespace Athena.Web.Sample.Home
{
    public class SendTestCommand
    {
        public Task Post(AthenaContext context)
        {
            return context.SendCommand(new TestCommand());
        }
    }
}