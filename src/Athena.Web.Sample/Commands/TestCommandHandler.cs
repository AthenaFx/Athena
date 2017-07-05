using System.Threading.Tasks;

namespace Athena.Web.Sample.Commands
{
    public class TestCommandHandler
    {
        public Task Handle(TestCommand command)
        {
            return Task.CompletedTask;
        }
    }
}