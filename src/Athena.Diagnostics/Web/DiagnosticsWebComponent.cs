using System.Collections.Generic;
using System.Reflection;
using Athena.Configuration;
using Athena.Diagnostics.Web.Endpoints.Home;
using Athena.Web;
using Athena.Web.Routing;

namespace Athena.Diagnostics.Web
{
    public class DiagnosticsWebComponent : AthenaComponent
    {
        public AthenaBootstrapper Configure(AthenaBootstrapper bootstrapper)
        {
            return bootstrapper
                .UsingWebApplication("web_diagnostics")
                .Configure(x => x.WithBaseUrl("_diagnostics").BuildRoutesWith((settings, btsrpr) =>
                        DefaultRouteConventions.BuildRoutes(y => $"{settings.BaseUrl}/{y}",
                            y => y.Namespace == typeof(Index).Namespace,
                            new List<string>
                            {
                                "Step"
                            }, typeof(WebDiagnostics).GetTypeInfo().Assembly))
                    .ModifyApplication(builder => builder.Remove("SupplyMetaData")));
        }
    }
}