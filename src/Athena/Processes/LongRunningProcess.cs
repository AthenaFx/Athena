using System.Threading.Tasks;
using Athena.Configuration;

namespace Athena.Processes
{
    public interface LongRunningProcess
    {
        Task Start(AthenaContext context);
        Task Stop(AthenaContext context);
    }
}