using System.Threading.Tasks;

namespace Athena.Processes
{
    public interface LongRunningProcess
    {
        Task Start();
        Task Stop();
    }
}