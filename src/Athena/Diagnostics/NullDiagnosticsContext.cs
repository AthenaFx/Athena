using System.Threading.Tasks;

namespace Athena.Diagnostics
{
    public class NullDiagnosticsContext : DiagnosticsContext
    {
        public Task Finish()
        {
            return Task.CompletedTask;
        }
    }
}