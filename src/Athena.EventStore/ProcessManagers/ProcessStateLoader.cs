using System.Threading.Tasks;

namespace Athena.EventStore.ProcessManagers
{
    public interface ProcessStateLoader<TSTate, in TIdentity>
    {
        Task<TSTate> Load(TIdentity id);
    }
}