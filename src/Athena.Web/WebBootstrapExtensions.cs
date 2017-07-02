using System.Linq;
using System.Threading.Tasks;
using Athena.Configuration;
using Athena.PartialApplications;

namespace Athena.Web
{
    public static class WebBootstrapExtensions
    {
        private static bool _hasConfiguredWeb;
        
        public static PartConfiguration<WebApplicationsSettings> UsingWeb(this AthenaBootstrapper bootstrapper)
        {
            if (_hasConfiguredWeb)
                return bootstrapper.Configure<WebApplicationsSettings>();
            
            _hasConfiguredWeb = true;
                
            return bootstrapper
                .ConfigureWith<WebApplicationsSettings>((conf, context) =>
                {
                    var applications = conf.GetApplications();

                    foreach (var application in applications)
                        context.DefineApplication(application.Item3.Name, application.Item3.GetApplicationBuilder());
                        
                    context.DefineApplication("web", builder => builder
                        .First("RunPartialApplication", next => new RunPartialApplication(next, env =>
                        {
                            return applications
                                .OrderBy(x => x.Item1)
                                .Where(x => x.Item2(env))
                                .Select(x => x.Item3.Name)
                                .FirstOrDefault();
                        }).Invoke));
                        
                    return Task.CompletedTask;
                });
        }

        public static PartConfiguration<DefaultWebApplicationSettings> UsingDefaultWeb(
            this AthenaBootstrapper bootstrapper)
        {
            return bootstrapper
                .UsingWeb()
                .Child<DefaultWebApplicationSettings>((webSettings, defaultWebAppSettings) => 
                    webSettings.AddApplication(defaultWebAppSettings));
        }
    }
}