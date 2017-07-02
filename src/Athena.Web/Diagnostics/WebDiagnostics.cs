using System.Collections.Generic;
using System.Reflection;
using Athena.Configuration;
using Athena.Diagnostics;
using Athena.Web.Routing;

namespace Athena.Web.Diagnostics
{   
    public static class WebDiagnostics
    {
        public static PartConfiguration<WebApplicationSettings> WithWebUi(
            this PartConfiguration<DiagnosticsConfiguration> config)
        {
            return config
                .UsingWebApplication("diagnostics_web")
                .Configure(x => x.WithBaseUrl("_diagnostics").BuildRoutesWith((settings, bootstrapper) =>
                    DefaultRouteConventions.BuildRoutes(y => $"{settings.BaseUrl}/{y}",
                        y => y.Namespace == "Athena.Web.Diagnostics.Endpoints.Home",
                        new List<string>
                        {
                            "Step"
                        }, typeof(WebDiagnostics).GetTypeInfo().Assembly))
                    .ModifyApplication(builder => builder.Remove("SupplyMetaData")));
        }
    }
}