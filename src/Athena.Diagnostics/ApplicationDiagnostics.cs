using System.Collections.Generic;
using Athena.Configuration;

namespace Athena.Diagnostics
{
    public static class ApplicationDiagnostics
    {
        public static PartConfiguration<DiagnosticsConfiguration> Diagnostics(
            this AthenaBootstrapper bootstrapper)
        {
            return bootstrapper.Part<DiagnosticsConfiguration>();
        }

        public static DiagnosticsContext OpenDiagnosticsTimerContext(this DiagnosticsConfiguration settings, 
            IDictionary<string, object> environment, string step, string name)
        {
            return new TimerDiagnosticsContext(settings.DataManager, settings.MetricsManager, environment, step, name,
                environment);
        }
    }
}