using Athena.Configuration;
using Athena.Diagnostics;

namespace Athena.Web.Diagnostics
{   
    public static class WebDiagnostics
    {
        public static string BaseUrl { get; private set; }
        
        public static PartConfiguration<DiagnosticsConfiguration> WithUiAt(
            this PartConfiguration<DiagnosticsConfiguration> config, string baseUrl)
        {
            BaseUrl = baseUrl;
            
            config
                .UsingPlugin(new DiagnosticsWebPlugin(baseUrl))
                .UsingPlugin(new WebAppPlugin())
                .ConfigurePart(x => x.WithPartialApplication("diagnostics_web", 
                    environment => environment.GetRequest().Uri.LocalPath.StartsWith($"/{baseUrl}")));

            return config;
        }
    }
}