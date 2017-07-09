using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Athena.Diagnostics
{
    public class TimerDiagnosticsContext : DiagnosticsContext
    {
        private readonly Stopwatch _timer;
        private readonly string _application;
        private readonly string _requestId;
        private readonly DiagnosticsDataManager _dataManager;
        private readonly MetricsDataManager _metricsDataManager;
        private readonly string _step;
        private readonly string _name;
        private readonly IDictionary<string, object> _environment;
        
        public TimerDiagnosticsContext(DiagnosticsDataManager dataManager, MetricsDataManager metricsDataManager, 
            IDictionary<string, object> environment, string step, string name)
        {
            _dataManager = dataManager;
            _step = step;
            _name = name;
            _metricsDataManager = metricsDataManager;
            _timer = Stopwatch.StartNew();
            _environment = environment;
            _application = environment.GetCurrentApplication();
            _requestId = environment.GetRequestId();
        }

        public async Task Finish()
        {
            _timer.Stop();
            
            await _dataManager
                .AddDiagnostics(_application, "requests", _requestId,
                    new DiagnosticsData(_step, new Dictionary<string, string>
                    {
                        [_name] = _timer.Elapsed.ToString()
                    }), _environment).ConfigureAwait(false);

            await _metricsDataManager
                .ReportMetricsTotalValue(_application, $"{_name}duration", _timer.Elapsed.TotalMilliseconds, 
                    DateTime.UtcNow, _environment)
                .ConfigureAwait(false);
        }
    }
}