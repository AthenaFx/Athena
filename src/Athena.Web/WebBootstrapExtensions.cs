using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Athena.Configuration;
using Athena.PartialApplications;

namespace Athena.Web
{
    public static class WebBootstrapExtensions
    {
        private static bool _hasConfiguredWeb;
        
        internal static PartConfiguration<WebApplicationsRouterSettings> UsingWeb(this AthenaBootstrapper bootstrapper)
        {
            if (_hasConfiguredWeb)
                return bootstrapper.Configure<WebApplicationsRouterSettings>();
            
            _hasConfiguredWeb = true;
                
            return bootstrapper
                .ConfigureWith<WebApplicationsRouterSettings>((conf, context) =>
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

        public static PartConfiguration<WebApplicationSettings> UsingWebApplication(
            this AthenaBootstrapper bootstrapper, string name = "default_web")
        {
            var key = $"_web_application_{name}";
            
            var appConfiguration = bootstrapper
                .UsingWeb()
                .Child<WebApplicationSettings>((webSettings, webAppSettings, _) =>
                    webSettings.AddApplication(webAppSettings,
                        (env, settings) => string.IsNullOrEmpty(settings.BaseUrl)
                                           || env.GetRequest().Uri.LocalPath.StartsWith($"/{settings.BaseUrl}")), key)
                .Configure(x => x.WithName(name));

            appConfiguration.Child<WebApplicationRequestErrorSettings>(async (webAppSettings, errorSettings, context) =>
            {
                await context.DefineApplication($"{webAppSettings.Name}_error", errorSettings.GetApplicationBuilder())
                    .ConfigureAwait(false);

                return webAppSettings;
            }, $"{key}_error");

            appConfiguration.Child<WebApplicationRequestNotFoundSettings>(
                async (webAppSettings, notFoundSettings, context) =>
                {
                    await context
                        .DefineApplication($"{webAppSettings.Name}_missing", notFoundSettings.GetApplicationBuilder())
                        .ConfigureAwait(false);

                    return webAppSettings;
                }, $"{key}_missing");
            
            return appConfiguration;
        }

        public static WebApplicationSettings GetCurrentWebApplicationSettings(
            this IDictionary<string, object> environment)
        {
            var context = environment.GetAthenaContext();

            return context.GetSetting<WebApplicationSettings>(
                $"_web_application_{environment.GetCurrentApplication()}");
        }
    }
}