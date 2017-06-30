using System.Threading.Tasks;
using Athena.Configuration;

namespace Athena
{
    public interface AthenaPlugin
    {
        Task Bootstrap(AthenaSetupContext context);
        Task TearDown(AthenaContext context);
    }
}