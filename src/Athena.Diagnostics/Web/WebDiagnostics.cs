using Athena.Configuration;
using Athena.Web;

namespace Athena.Diagnostics.Web
{   
    public static class WebDiagnostics
    {
        public static PartConfiguration<WebApplicationSettings> WebUi(
            this PartConfiguration<DiagnosticsConfiguration> config)
        {
            return config.UsingWebApplication("web_diagnostics");
        }
    }
}