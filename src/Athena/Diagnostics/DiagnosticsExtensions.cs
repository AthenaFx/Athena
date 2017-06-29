using System.Collections.Generic;
using Athena.Messages;
using Athena.PubSub;

namespace Athena.Diagnostics
{
    public static class DiagnosticsExtensions
    {
        public static AthenaBootstrapper EnableDiagnostics(this AthenaBootstrapper bootstrapper,
            DiagnosticsDataManager diagnosticsDataManager)
        {
            foreach (var application in bootstrapper.GetDefinedApplications())
            {
                bootstrapper.ConfigureApplication(application, builder => builder.WrapAllWith((next, nextItem) =>
                    new DiagnoseInnerBehavior(next, nextItem, diagnosticsDataManager).Invoke));
            }
            
            EventPublishing.Subscribe<BootstrapCompleted>(async x =>
            {
                await diagnosticsDataManager.AddDiagnostics("Setup", DiagnosticsTypes.Bootstrapping, "Init",
                    new DiagnosticsData("BootstrapCompleted", new Dictionary<string, DiagnosticsValue>
                    {
                        ["ExecutionTime"] = new ObjectDiagnosticsValue(x.ExecutionTime),
                        ["ApplicationName"] = new ObjectDiagnosticsValue(bootstrapper.ApplicationName)
                    })).ConfigureAwait(false);
            });

            return bootstrapper;
        }
    }
}