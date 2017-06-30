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
        void DefineApplication(string name, Func<AppFunctionBuilder, AppFunctionBuilder> builder);
        void ConfigureApplication(string name, Func<AppFunctionBuilder, AppFunctionBuilder> builder);
        IReadOnlyCollection<string> GetDefinedApplications();
        Task Done(SetupEvent evnt);
    }
}