using System.Threading.Tasks;

namespace Athena.EventStore.ProcessManagers
{
    public interface ProcessStateLoader
    {
        Task<TSTate> Load<TSTate, TIdentity>(TIdentity id) where TSTate : new();
    }
}