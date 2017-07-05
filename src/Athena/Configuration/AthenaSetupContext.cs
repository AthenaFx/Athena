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
        IReadOnlyCollection<Assembly> ApplicationAssemblies { get; }
        Task DefineApplication(string name, Func<AppFunctionBuilder, AppFunctionBuilder> builder);
        Task UpdateApplication(string name, Func<AppFunctionBuilder, AppFunctionBuilder> builder);
    }
}