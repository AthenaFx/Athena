using System.Threading.Tasks;

namespace Athena
{
    public interface AthenaPlugin
    {
        Task Bootstrap(AthenaBootstrapper context);
        Task TearDown(AthenaBootstrapper context);
    }
}