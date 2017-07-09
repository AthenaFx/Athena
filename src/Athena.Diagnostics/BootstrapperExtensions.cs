using Athena.Configuration;

namespace Athena.Diagnostics
{
    public static class BootstrapperExtensions
    {
        public static PartConfiguration<DiagnosticsConfiguration> EnabledDiagnostics(
            this AthenaBootstrapper bootstrapper)
        {
            return bootstrapper.Part<DiagnosticsConfiguration>();
        }
    }
}