using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena.Diagnostics
{
    public class DiagnosticsConfiguration
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, double>> _tolerableApdexValues = 
            new ConcurrentDictionary<string, ConcurrentDictionary<string, double>>();

        private Func<IDictionary<string, object>, bool> _enabledCheck;

        private DiagnosticsDataManager _dataManager = new InMemoryDiagnosticsDataManager();
        
        private MetricsDataManager _metricsManager = new InMemoryMetricsDataManager(TimeSpan.FromDays(1));

        public Func<IDictionary<string, object>, bool> HasError { get; private set; } =
            env => env.Get<Exception>("exception") != null;

        public DiagnosticsConfiguration UsingDataManager(DiagnosticsDataManager dataManager)
        {
            _dataManager = dataManager;

            return this;
        }
        
        public DiagnosticsConfiguration UsingMetricsDataManager(MetricsDataManager dataManager)
        {
            _metricsManager = dataManager;

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

        public DiagnosticsConfiguration EnabledWhen(Func<IDictionary<string, object>, bool> enabledCheck)
        {
            _enabledCheck = enabledCheck;

            return this;
        }

        public DiagnosticsDataManager GetDiagnosticsDataManager()
        {
            return new DiagnosticsDataManagerWrapper(_dataManager, _enabledCheck ?? (_ => true));
        }

        public MetricsDataManager GetMetricsDataManager()
        {
            return new MetricsDataManagerWrapper(_metricsManager, _enabledCheck ?? (_ => true));
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
            private readonly Func<IDictionary<string, object>, bool> _enabledCheck;

            public DiagnosticsDataManagerWrapper(DiagnosticsDataManager inner, 
                Func<IDictionary<string, object>, bool> enabledCheck)
            {
                _inner = inner;
                _enabledCheck = enabledCheck;
            }

            public Task AddDiagnostics(string application, string type, string step, DiagnosticsData data,
                IDictionary<string, object> environment)
            {
                if (!_enabledCheck(environment))
                    return Task.CompletedTask;
                
                return _inner.AddDiagnostics(application, type, step, data, environment);
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
            private readonly Func<IDictionary<string, object>, bool> _enabledCheck;

            public MetricsDataManagerWrapper(MetricsDataManager inner, 
                Func<IDictionary<string, object>, bool> enabledCheck)
            {
                _inner = inner;
                _enabledCheck = enabledCheck;
            }

            public Task ReportMetricsTotalValue(string application, string key, double value, DateTime at,
                IDictionary<string, object> environment)
            {
                if (!_enabledCheck(environment))
                    return Task.CompletedTask;
                
                return _inner.ReportMetricsTotalValue(application, key, value, at, environment);
            }

            public Task ReportMetricsPerSecondValue(string application, string key, double value, DateTime at,
                IDictionary<string, object> environment)
            {
                if (!_enabledCheck(environment))
                    return Task.CompletedTask;
                
                return _inner.ReportMetricsPerSecondValue(application, key, value, at, environment);
            }

            public Task ReportMetricsApdexValue(string application, string key, double value, DateTime at, 
                double tolerable, IDictionary<string, object> environment)
            {                
                if (!_enabledCheck(environment))
                    return Task.CompletedTask;
                
                return _inner.ReportMetricsApdexValue(application, key, value, at, tolerable, environment);
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