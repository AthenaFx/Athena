using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Athena.Configuration;
using Athena.FeatureFlags;
using Athena.Logging;
using Athena.PubSub;

namespace Athena.Diagnostics
{
    public class DiagnosticsComponent : AthenaComponent
    {
        public AthenaBootstrapper Configure(AthenaBootstrapper bootstrapper)
        {
            return bootstrapper
                .Features()
                .Configure(x => x.WithDefaultFeature(bootstrapper.ApplicationName, new OnFeatureFlagCalculator())
                    .WithDefaultFeature("diagnostics", new ForEnvironmentsFeatureFlagCalculator("dev", "test")))
                .Part<DiagnosticsConfiguration>()
                .On<ApplicationDefined>((conf, evnt, context) =>
                {
                    Logger.Write(LogLevel.Debug, "Configuring diagnostics");

                    bootstrapper
                        .Features()
                        .Configure(x => x.WithDefaultFeature($"metrics-{evnt.Name}",
                            new OnFeatureFlagCalculator()));
                    
                    return context.UpdateApplication(evnt.Name,
                        builder => builder.First("ReportErrorRate", 
                                next => new ReportErrorRate(next, conf.HasError, conf.MetricsManager).Invoke)
                            .WrapAllWith((next, nextItem) =>
                                new DiagnoseInnerBehavior(next, nextItem, conf).Invoke));
                }).On<ApplicationCompiled>(async (conf, evnt, context) =>
                {
                    await Task.WhenAll(evnt.Data.Select(item =>
                            conf
                                .DataManager
                                .AddDiagnostics(evnt.Name, "Configuration", "Definition",
                                    new DiagnosticsData(item.Key, item.Value), context.SetupEnvironment)))
                        .ConfigureAwait(false);
                }).On<SetupEvent>(async (conf, evnt, context) =>
                {
                    var data = evnt
                        .GetType()
                        .GetTypeInfo()
                        .GetProperties()
                        .Where(ShouldIncludeInDiagnostics)
                        .ToDictionary(x => x.Name, x => x.GetValue(evnt).ToString());
                
                    await conf
                        .DataManager
                        .AddDiagnostics(context.ApplicationName, "Lifecycle", "Startup",
                            new DiagnosticsData($"{evnt.GetType().Name}-{Guid.NewGuid():N}", data), 
                            context.SetupEnvironment)
                        .ConfigureAwait(false);
                }).OnStartup((conf, context) =>
                {
                    EventPublishing.OpenChannel<object>()
                        .Select(async evnt =>
                        {
                            var data = IntrospectionExtensions.GetTypeInfo(evnt.Event
                                    .GetType())
                                .GetProperties()
                                .Where(ShouldIncludeInDiagnostics)
                                .ToDictionary(x => x.Name, x => x.GetValue(evnt.Event).ToString());

                            await conf
                                .DataManager
                                .AddDiagnostics(context.ApplicationName, "Lifecycle", "Runtime",
                                    new DiagnosticsData($"{evnt.GetType().Name}-{Guid.NewGuid():N}", data),
                                    evnt.Environment)
                                .ConfigureAwait(false);
                        }).Subscribe();

                    EventPublishing.OpenChannel<ApplicationExecutedRequest>()
                        .Select(async evnt =>
                        {
                            await conf
                                .MetricsManager
                                .ReportMetricsTotalValue(evnt.Event.Application, "requestduration",
                                    evnt.Event.Duration.TotalMilliseconds,
                                    evnt.Event.At, evnt.Environment)
                                .ConfigureAwait(false);

                            await conf
                                .MetricsManager
                                .ReportMetricsApdexValue(evnt.Event.Application, "requestdurationapdex",
                                    evnt.Event.Duration.TotalMilliseconds, evnt.Event.At,
                                    conf.GetTolerableApdexValue(evnt.Event.Application, "requestdurationapdex"),
                                    evnt.Environment)
                                .ConfigureAwait(false);

                            await conf
                                .MetricsManager
                                .ReportMetricsPerSecondValue(evnt.Event.Application, "requestrate", 1,
                                    evnt.Event.At, evnt.Environment)
                                .ConfigureAwait(false);
                        }).Subscribe();
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