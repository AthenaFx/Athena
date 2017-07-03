using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Athena.Configuration;
using Athena.Logging;
using Athena.PubSub;

namespace Athena.Diagnostics
{
    public static class ApplicationDiagnostics
    {
        public static PartConfiguration<DiagnosticsConfiguration> EnableDiagnostics(
            this AthenaBootstrapper bootstrapper)
        {
            Logger.Write(LogLevel.Debug, $"Enabling diagnostics");
            
            EventPublishing.Subscribe<SetupEvent>(async (evnt, context) =>
            {
                var data = evnt
                    .GetType()
                    .GetProperties()
                    .Where(ShouldIncludeInDiagnostics)
                    .ToDictionary(x => x.Name, x => x.GetValue(evnt).ToString());
                
                await context.GetSetting<DiagnosticsConfiguration>()
                    .DataManager
                    .AddDiagnostics("Setup", DiagnosticsTypes.Bootstrapping, "Init",
                    new DiagnosticsData($"{evnt.GetType().Name}-{Guid.NewGuid():N}", data)).ConfigureAwait(false);
            });

            EventPublishing.Subscribe<ShutdownEvent>(async (evnt, context) =>
            {
                var data = evnt
                    .GetType()
                    .GetProperties()
                    .Where(ShouldIncludeInDiagnostics)
                    .ToDictionary(x => x.Name, x => x.GetValue(evnt).ToString());
                
                await context.GetSetting<DiagnosticsConfiguration>()
                    .DataManager
                    .AddDiagnostics("Setup", DiagnosticsTypes.Shutdown, "Shutdown",
                    new DiagnosticsData($"{evnt.GetType().Name}-{Guid.NewGuid():N}", data)).ConfigureAwait(false);
            });

            EventPublishing.Subscribe<ApplicationCompiled>(async (evnt, context) =>
            {
                await Task.WhenAll(evnt.Data.Select(item =>
                    context.GetSetting<DiagnosticsConfiguration>()
                        .DataManager
                        .AddDiagnostics("Applications", DiagnosticsTypes.Bootstrapping, evnt.Name,
                            new DiagnosticsData(item.Key, item.Value))))
                    .ConfigureAwait(false);
            });

            return bootstrapper
                .ConfigureWith<DiagnosticsConfiguration, ApplicationDefined>((conf, evnt, context) =>
                {
                    Logger.Write(LogLevel.Debug, $"Configuring diagnostics");
                    
                    return context.UpdateApplication(evnt.Name,
                        builder => builder.WrapAllWith((next, nextItem) =>
                            new DiagnoseInnerBehavior(next, nextItem, conf.DataManager).Invoke));
                });
        }

        private static bool ShouldIncludeInDiagnostics(PropertyInfo propertyInfo)
        {
            var propertyTypeInfo = propertyInfo.PropertyType.GetTypeInfo();

            return propertyInfo.CanRead
                   && (propertyTypeInfo.IsPrimitive 
                       || propertyTypeInfo.IsEnum 
                       || propertyInfo.PropertyType == typeof(string)
                       || propertyInfo.PropertyType == typeof(DateTime)
                       || propertyInfo.PropertyType == typeof(TimeSpan));
        }
    }
}