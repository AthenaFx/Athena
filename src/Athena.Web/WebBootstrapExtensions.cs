using System;
using System.Collections.Generic;
using Athena.Configuration;
using Athena.Logging;

namespace Athena.Web
{
    public static class WebBootstrapExtensions
    {
        private static readonly ICollection<string> RegisteredWebApplications = new List<string>();
        
        public static PartConfiguration<WebApplicationSettings> UsingWebApplication(
            this AthenaBootstrapper bootstrapper, string name = "web_default")
        {
            var key = $"_web_application_{name}";

            if (RegisteredWebApplications.Contains(key))
            {
                return bootstrapper
                    .Part<WebApplicationSettings>(key);
            }

            Logger.Write(LogLevel.Debug, $"Adding web application named {name}");
            
            var appConfiguration = bootstrapper
                .Part<WebApplicationsRouterSettings>()
                .Child<WebApplicationSettings>(key)
                .ConfigureParentWith((webSettings, webAppSettings, _) => webAppSettings.Disabled ? webSettings :
                    webSettings.AddApplication(webAppSettings,
                        !string.IsNullOrEmpty(webAppSettings.BaseUrl) 
                            ? (env, settings) => env.GetRequest().Uri.LocalPath.StartsWith($"/{settings.BaseUrl}") 
                            : (Func<IDictionary<string, object>, WebApplicationSettings, bool>)null))
                .Configure(x => x.WithName(name));

            appConfiguration.Child<WebApplicationRequestErrorSettings>($"{key}_error")
                .ConfigureParentWith(async (webAppSettings, errorSettings, context) =>
                {
                    await context.DefineApplication($"{webAppSettings.Name}_error", errorSettings.GetApplicationBuilder())
                        .ConfigureAwait(false);

                    return webAppSettings;
                });
            
            appConfiguration.Child<WebApplicationRequestNotFoundSettings>($"{key}_missing")
                .ConfigureParentWith(async (webAppSettings, notFoundSettings, context) =>
                {
                    await context
                        .DefineApplication($"{webAppSettings.Name}_missing", notFoundSettings.GetApplicationBuilder())
                        .ConfigureAwait(false);

                    return webAppSettings;
                });
            
            appConfiguration.Child<WebApplicationRequestUnAuthorizedSettings>($"{key}_unauthorized")
                .ConfigureParentWith(async (webAppSettings, notFoundSettings, context) =>
                {
                    await context
                        .DefineApplication($"{webAppSettings.Name}_unauthorized", notFoundSettings.GetApplicationBuilder())
                        .ConfigureAwait(false);

                    return webAppSettings;
                });
            
            appConfiguration.Child<WebApplicationRequestValidationErrorSettings>($"{key}_invalid")
                .ConfigureParentWith(async (webAppSettings, notFoundSettings, context) =>
                {
                    await context
                        .DefineApplication($"{webAppSettings.Name}_invalid", notFoundSettings.GetApplicationBuilder())
                        .ConfigureAwait(false);

                    return webAppSettings;
                });

            RegisteredWebApplications.Add(key);
            
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