using System.Linq;
using System.Threading.Tasks;
using Athena.Configuration;
using Athena.Logging;
using Athena.PartialApplications;

namespace Athena.Web
{
    public class WebAthenaComponent : AthenaComponent
    {
        public AthenaBootstrapper Configure(AthenaBootstrapper bootstrapper)
        {
            return bootstrapper
                .Part<WebApplicationsRouterSettings>()
                .OnSetup((conf, context) =>
                {
                    var applications = conf.GetApplications().OrderBy(x => x.Item1).ToList();

                    Logger.Write(LogLevel.Debug, $"Configuring {applications.Count} web applications");
                    
                    foreach (var application in applications)
                        context.DefineApplication(application.Item3.Name, application.Item3.GetApplicationBuilder());

                    var diagnosticsData = applications
                        .ToDictionary(x => x.Item3.Name, x => $"/{x.Item3.BaseUrl}");
                        
                    context.DefineApplication("web", builder => builder
                        .First("RunPartialApplication", next => new RunPartialApplication(next, env =>
                        {
                            return applications
                                .OrderBy(x => x.Item1)
                                .Where(x => x.Item2(env))
                                .Select(x => x.Item3.Name)
                                .FirstOrDefault();
                        }).Invoke, () => diagnosticsData));
                        
                    return Task.CompletedTask;
                });
        }
    }
}