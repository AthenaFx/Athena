using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Athena
{
    public interface AthenaContext
    {
        IReadOnlyCollection<Assembly> ApplicationAssemblies { get; }
        string ApplicationName { get; }
        string Environment { get; }
        Task Execute(string application, IDictionary<string, object> environment);
        Task ShutDown();
    }
}