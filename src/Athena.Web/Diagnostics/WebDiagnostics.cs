using Athena.Configuration;
using Athena.Diagnostics;

namespace Athena.Web.Diagnostics
{   
    public static class WebDiagnostics
    {
        public static PartConfiguration<DiagnosticsWebApplicationSettings> WithWebUi(
            this PartConfiguration<DiagnosticsConfiguration> config)
        {
            return config
                .UsingWeb()
                .Child<DiagnosticsWebApplicationSettings>((webSettings, diagnosticsSettings) =>
                    webSettings.AddApplication(diagnosticsSettings,
                        (env, settings) => env.GetRequest().Uri.LocalPath.StartsWith($"/{settings.BaseUrl}")));
        }
    }
}