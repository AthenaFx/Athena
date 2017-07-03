using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Athena.Configuration;
using Athena.Logging;
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
            
            Logger.Write(LogLevel.Debug, $"Enabling web applications");
            
            _hasConfiguredWeb = true;
                
            return bootstrapper
                .ConfigureWith<WebApplicationsRouterSettings>((conf, context) =>
                {
                    var applications = conf.GetApplications();

                    Logger.Write(LogLevel.Debug, $"Configuring {applications.Count} web applications");
                    
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
            
            Logger.Write(LogLevel.Debug, $"Adding web application named {name}");
            
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
            
            appConfiguration.Child<WebApplicationRequestUnAuthorizedSettings>(
                async (webAppSettings, notFoundSettings, context) =>
                {
                    await context
                        .DefineApplication($"{webAppSettings.Name}_unauthorized", notFoundSettings.GetApplicationBuilder())
                        .ConfigureAwait(false);

                    return webAppSettings;
                }, $"{key}_unauthorized");
            
            appConfiguration.Child<WebApplicationRequestValidationErrorSettings>(
                async (webAppSettings, notFoundSettings, context) =>
                {
                    await context
                        .DefineApplication($"{webAppSettings.Name}_invalid", notFoundSettings.GetApplicationBuilder())
                        .ConfigureAwait(false);

                    return webAppSettings;
                }, $"{key}_invalid");
            
            return appConfiguration;
        }

        public static PartConfiguration<WebApplicationRequestErrorSettings> OnError(
            this PartConfiguration<WebApplicationSettings> settings)
        {
            var key = $"_web_application_{settings.Settings.Name}_error";

            return settings.Child<WebApplicationRequestErrorSettings>(key);
        }
        
        public static PartConfiguration<WebApplicationRequestNotFoundSettings> OnMissing(
            this PartConfiguration<WebApplicationSettings> settings)
        {
            var key = $"_web_application_{settings.Settings.Name}_missing";

            return settings.Child<WebApplicationRequestNotFoundSettings>(key);
        }
        
        public static PartConfiguration<WebApplicationRequestUnAuthorizedSettings> OnUnAuthorized(
            this PartConfiguration<WebApplicationSettings> settings)
        {
            var key = $"_web_application_{settings.Settings.Name}_unauthorized";

            return settings.Child<WebApplicationRequestUnAuthorizedSettings>(key);
        }
        
        public static PartConfiguration<WebApplicationRequestValidationErrorSettings> OnValidationFailure(
            this PartConfiguration<WebApplicationSettings> settings)
        {
            var key = $"_web_application_{settings.Settings.Name}_invalid";

            return settings.Child<WebApplicationRequestValidationErrorSettings>(key);
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