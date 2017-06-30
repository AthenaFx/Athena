using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Configuration;
using Athena.PubSub;

namespace Athena.Diagnostics
{
    public static class DiagnosticsExtensions
    {
        public static AthenaBootstrapper EnableDiagnostics(this AthenaBootstrapper bootstrapper,
            DiagnosticsDataManager diagnosticsDataManager)
        {
            return bootstrapper
                .When<SetupEvent>()
                .Do(async (evnt, context) =>
                    {
                        await diagnosticsDataManager.AddDiagnostics("Setup", DiagnosticsTypes.Bootstrapping, "Init",
                            new DiagnosticsData(evnt.GetType().Name, new Dictionary<string, DiagnosticsValue>
                            {
                                ["ExecutionTime"] = new ObjectDiagnosticsValue(evnt.ExecutionTime),
                                ["ApplicationName"] = new ObjectDiagnosticsValue(context.ApplicationName),
                                ["Environment"] = new ObjectDiagnosticsValue(context.Environment)
                            })).ConfigureAwait(false);
                    })
                .When<AllPluginsBootstrapped>()
                .Do((evnt, context) =>
                {
                    foreach (var application in context.GetDefinedApplications())
                    {
                        context.ConfigureApplication(application, 
                            builder => builder.WrapAllWith((next, nextItem) =>
                                new DiagnoseInnerBehavior(next, nextItem, diagnosticsDataManager).Invoke));
                    }
                    
                    return Task.CompletedTask;
                });
        }
    }
}