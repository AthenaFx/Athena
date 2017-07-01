using System;
using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace Athena.EventStore.ProcessManagers
{
    public class LoadProcessManagerStateFromEventStore : ProcessStateLoader
    {
        private readonly IEventStoreConnection _connection;

        public LoadProcessManagerStateFromEventStore(IEventStoreConnection connection)
        {
            _connection = connection;
        }

        public Task<TSTate> Load<TSTate, TIdentity>(TIdentity id) where TSTate : new()
        {
            //TODO:Impliment
            throw new NotImplementedException();
        }
    }
}