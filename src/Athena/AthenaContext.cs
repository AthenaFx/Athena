using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Athena
{
    public interface AthenaContext
    {
        string ApplicationName { get; }
        string Environment { get; }
        IReadOnlyCollection<Assembly> ApplicationAssemblies { get; }
        TSetting GetSetting<TSetting>(string key = null) where TSetting : class;
        object GetSetting(Type type, string key = null);
        Task Execute(string application, IDictionary<string, object> environment);
        Task ShutDown();
    }
}