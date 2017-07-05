using System;
using System.Collections.Generic;

namespace Athena.Diagnostics
{
    public class DiagnosticsConfiguration
    {
        public DiagnosticsDataManager DataManager { get; private set; } = new InMemoryDiagnosticsDataManager();
        public MetricsDataManager MetricsManager { get; private set; } = 
            new InMemoryMetricsDataManager(TimeSpan.FromDays(1));

        public Func<IDictionary<string, object>, bool> HasError { get; private set; } =
            (env => env.Get<Exception>("exception") != null);

        public DiagnosticsConfiguration UsingDataManager(DiagnosticsDataManager dataManager)
        {
            DataManager = dataManager;

            return this;
        }
        
        public DiagnosticsConfiguration UsingMetricsDataManager(MetricsDataManager dataManager)
        {
            MetricsManager = dataManager;

            return this;
        }

        public DiagnosticsConfiguration CheckIfRequestHasErrorUsing(Func<IDictionary<string, object>, bool> hasError)
        {
            HasError = hasError;

            return this;
        }
    }
}