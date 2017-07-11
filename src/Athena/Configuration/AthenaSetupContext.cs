using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Athena.Configuration
{
    public interface AthenaSetupContext
    {
        string ApplicationName { get; }
        string Environment { get; }
        IDictionary<string, object> SetupEnvironment { get; }
        IReadOnlyCollection<Assembly> ApplicationAssemblies { get; }
        void AddTiming(string key, TimeSpan? elapsed = null);
        Task DefineApplication(string name, Func<AppFunctionBuilder, AppFunctionBuilder> builder);
        Task UpdateApplication(string name, Func<AppFunctionBuilder, AppFunctionBuilder> builder);
    }
}