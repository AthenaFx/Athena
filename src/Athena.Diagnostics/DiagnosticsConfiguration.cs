﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.FeatureFlags;

namespace Athena.Diagnostics
{
    public class DiagnosticsConfiguration
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, double>> _tolerableApdexValues = 
            new ConcurrentDictionary<string, ConcurrentDictionary<string, double>>();

        public DiagnosticsDataManager DataManager { get; private set; } =
            new DiagnosticsDataManagerWrapper(new InMemoryDiagnosticsDataManager());
        
        public MetricsDataManager MetricsManager { get; private set; } = 
            new MetricsDataManagerWrapper(new InMemoryMetricsDataManager(TimeSpan.FromDays(1)));

        public Func<IDictionary<string, object>, bool> HasError { get; private set; } =
            env => env.Get<Exception>("exception") != null;

        public DiagnosticsConfiguration UsingDataManager(DiagnosticsDataManager dataManager)
        {
            DataManager = new DiagnosticsDataManagerWrapper(dataManager);

            return this;
        }
        
        public DiagnosticsConfiguration UsingMetricsDataManager(MetricsDataManager dataManager)
        {
            MetricsManager = new MetricsDataManagerWrapper(dataManager);

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
        
        private class DiagnosticsDataManagerWrapper : DiagnosticsDataManager
        {
            private readonly DiagnosticsDataManager _inner;

            public DiagnosticsDataManagerWrapper(DiagnosticsDataManager inner)
            {
                _inner = inner;
            }

            public async Task AddDiagnostics(string application, string type, string step, DiagnosticsData data,
                IDictionary<string, object> environment)
            {
                var context = environment.GetAthenaContext();

                var featureStore = context?.GetSetting<FeaturesSettings>()?.FeatureStore;

                if (featureStore != null && !featureStore
                        .IsOn($"diagnostics-{application}-{type}-{step}", environment))
                {
                    return;
                }

                await _inner.AddDiagnostics(application, type, step, data, environment).ConfigureAwait(false);
            }

            public Task<IEnumerable<string>> GetApplications()
            {
                return _inner.GetApplications();
            }

            public Task<IEnumerable<string>> GetTypesFor(string application)
            {
                return _inner.GetTypesFor(application);
            }

            public Task<IEnumerable<string>> GetStepsFor(string application, string type, int numberOfSteps = 50)
            {
                return _inner.GetStepsFor(application, type, numberOfSteps);
            }

            public Task<IReadOnlyDictionary<string, IEnumerable<KeyValuePair<string, string>>>> GetDataFor(
                string application, string type, string step)
            {
                return _inner.GetDataFor(application, type, step);
            }
        }
        
        private class MetricsDataManagerWrapper : MetricsDataManager
        {
            private readonly MetricsDataManager _inner;

            public MetricsDataManagerWrapper(MetricsDataManager inner)
            {
                _inner = inner;
            }

            public async Task ReportMetricsTotalValue(string application, string key, double value, DateTime at,
                IDictionary<string, object> environment)
            {
                var context = environment.GetAthenaContext();

                var featureStore = context?.GetSetting<FeaturesSettings>()?.FeatureStore;

                if (featureStore != null && !featureStore.IsOn($"metrics-{application}-{key}", environment))
                {
                    return;
                }

                await _inner.ReportMetricsTotalValue(application, key, value, at, environment).ConfigureAwait(false);
            }

            public async Task ReportMetricsPerSecondValue(string application, string key, double value, DateTime at,
                IDictionary<string, object> environment)
            {
                var context = environment.GetAthenaContext();

                var featureStore = context?.GetSetting<FeaturesSettings>()?.FeatureStore;

                if (featureStore != null && !featureStore.IsOn($"metrics-{application}-{key}", environment))
                {
                    return;
                }

                await _inner.ReportMetricsPerSecondValue(application, key, value, at, environment)
                    .ConfigureAwait(false);
            }

            public async Task ReportMetricsApdexValue(string application, string key, double value, DateTime at, 
                double tolerable, IDictionary<string, object> environment)
            {
                var context = environment.GetAthenaContext();

                var featureStore = context?.GetSetting<FeaturesSettings>()?.FeatureStore;

                if (featureStore != null && !featureStore.IsOn($"metrics-{application}-{key}", environment))
                {
                    return;
                }
                
                await _inner.ReportMetricsApdexValue(application, key, value, at, tolerable, environment)
                    .ConfigureAwait(false);
            }

            public Task<double> GetAverageFor(string application, string key)
            {
                return _inner.GetAverageFor(application, key);
            }

            public Task<IReadOnlyCollection<string>> GetKeys(string application)
            {
                return _inner.GetKeys(application);
            }
        }
    }
}