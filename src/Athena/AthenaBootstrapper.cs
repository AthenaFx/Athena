using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena
{
    public interface AthenaBootstrapper
    {
        string ApplicationName { get; }
        AthenaBootstrapper DefineApplication(string name, Func<AppFunctionBuilder, AppFunctionBuilder> app, bool overwrite = true);
        AthenaBootstrapper ConfigureApplication(string name, Func<AppFunctionBuilder, AppFunctionBuilder> app);
        AthenaBootstrapper WithApplicationName(string name);
        IReadOnlyCollection<string> GetDefinedApplications();
    }
}