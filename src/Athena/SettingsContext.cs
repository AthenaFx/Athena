using System.Collections.Generic;
using System.Reflection;

namespace Athena
{
    public interface SettingsContext
    {
        IReadOnlyCollection<Assembly> ApplicationAssemblies { get; }
        string ApplicationName { get; }
        string Environment { get; }
        TSetting GetSetting<TSetting>(string key = null) where TSetting : class;   
    }
}