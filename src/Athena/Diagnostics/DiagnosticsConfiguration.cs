using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Athena.Diagnostics
{
    public class DiagnosticsConfiguration
    {
        private ConcurrentDictionary<string, ConcurrentDictionary<string, double>> _tolerableApdexValues = 
            new ConcurrentDictionary<string, ConcurrentDictionary<string, double>>();
        
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

        public DiagnosticsConfiguration TolerableValueIs(string application, string key, double value)
        {
            _tolerableApdexValues.GetOrAdd(application, x => new ConcurrentDictionary<string, double>())
                .AddOrUpdate(key, x => value, (_, __) => value);

            return this;
        }

        internal double GetTolerableApdexValue(string application, string key)
        {
            if (!_tolerableApdexValues.ContainsKey(application))
                return 500;

            if (!_tolerableApdexValues[application].ContainsKey(key))
                return 500;

            return _tolerableApdexValues[application][key];
        }
    }
}