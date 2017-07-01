using System.Threading.Tasks;

namespace Athena.EventStore.Projections
{
    //TODO:Impliment
    public class StoreProjectionsPositionOnDisc : ProjectionsPositionHandler
    {
        public Task<long> GetLastEvent(string projection)
        {
            throw new System.NotImplementedException();
        }

        public Task SetLastEvent(string projection, long eventNumber)
        {
            throw new System.NotImplementedException();
        }
    }
}