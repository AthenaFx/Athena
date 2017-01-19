using System.Threading.Tasks;

namespace Athena
{
    public interface AthenaPlugin
    {
        Task Start(AthenaContext context);
        Task ShutDown(AthenaContext context);
    }
}