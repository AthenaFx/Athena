using Athena.Configuration;

namespace Athena.Diagnostics
{
    public static class BootstrapperExtensions
    {
        public static PartConfiguration<DiagnosticsConfiguration> EnableDiagnostics(
            this AthenaBootstrapper bootstrapper)
        {
            return bootstrapper.Part<DiagnosticsConfiguration>();
        }
    }
}