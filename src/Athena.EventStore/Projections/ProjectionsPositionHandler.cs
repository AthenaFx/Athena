using System.Threading.Tasks;

namespace Athena.EventStore.Projections
{
    public interface ProjectionsPositionHandler
    {
        Task<long> GetLastEvent(string projection);
        Task SetLastEvent(string projection, long eventNumber);
    }
}