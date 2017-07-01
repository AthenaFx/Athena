using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena
{
    public interface AthenaContext : SettingsContext
    {
        Task Execute(string application, IDictionary<string, object> environment);
        Task ShutDown();
    }
}