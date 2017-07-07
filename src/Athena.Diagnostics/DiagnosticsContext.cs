using System.Threading.Tasks;

namespace Athena.Diagnostics
{
    public interface DiagnosticsContext
    {
        Task Finish();
    }
}