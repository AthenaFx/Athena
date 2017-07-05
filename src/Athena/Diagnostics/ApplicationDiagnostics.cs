using System;
using System.Collections.Generic;
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
            Logger.Write(LogLevel.Debug, "Enabling diagnostics");

            return bootstrapper
                .Part<DiagnosticsConfiguration>()
                .On<ApplicationDefined>((conf, evnt, context) =>
                {
                    Logger.Write(LogLevel.Debug, "Configuring diagnostics");
                    
                    return context.UpdateApplication(evnt.Name,
                        builder => builder.First("ReportErrorRate", 
                                next => new ReportErrorRate(next, conf.HasError, conf.MetricsManager).Invoke)
                            .WrapAllWith((next, nextItem) =>
                            new DiagnoseInnerBehavior(next, nextItem, conf.DataManager).Invoke));
                }).On<ApplicationCompiled>(async (conf, evnt, context) =>
                {
                    await Task.WhenAll(evnt.Data.Select(item =>
                            conf
                                .DataManager
                                .AddDiagnostics(evnt.Name, "Configuration", "Definition",
                                    new DiagnosticsData(item.Key, item.Value))))
                        .ConfigureAwait(false);
                }).On<SetupEvent>(async (conf, evnt, context) =>
                {
                    var data = evnt
                        .GetType()
                        .GetProperties()
                        .Where(ShouldIncludeInDiagnostics)
                        .ToDictionary(x => x.Name, x => x.GetValue(evnt).ToString());
                
                    await conf
                        .DataManager
                        .AddDiagnostics(context.ApplicationName, "Lifecycle", "Startup",
                            new DiagnosticsData($"{evnt.GetType().Name}-{Guid.NewGuid():N}", data))
                        .ConfigureAwait(false);
                }).OnStartup((conf, context) =>
                {
                    EventPublishing.Subscribe<object>(async evnt =>
                    {
                        var data = evnt
                            .GetType()
                            .GetProperties()
                            .Where(ShouldIncludeInDiagnostics)
                            .ToDictionary(x => x.Name, x => x.GetValue(evnt).ToString());
                
                        await conf
                            .DataManager
                            .AddDiagnostics(context.ApplicationName, "Lifecycle", "Runtime",
                                new DiagnosticsData($"{evnt.GetType().Name}-{Guid.NewGuid():N}", data))
                            .ConfigureAwait(false);
                    });
                });
        }

        public static DiagnosticsContext OpenDiagnosticsTimerContext(this DiagnosticsDataManager dataManager, 
            IDictionary<string, object> environment, string step, string name)
        {
            return new TimerDiagnosticsContext(dataManager, environment, step, name);
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