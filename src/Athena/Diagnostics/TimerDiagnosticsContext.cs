﻿using System.Collections.Generic;
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
        private readonly string _step;
        private readonly string _name;
        
        public TimerDiagnosticsContext(DiagnosticsDataManager dataManager, IDictionary<string, object> environment, 
            string step, string name)
        {
            _dataManager = dataManager;
            _step = step;
            _name = name;
            _timer = Stopwatch.StartNew();
            _application = environment.GetCurrentApplication();
            _requestId = environment.GetRequestId();
        }

        public async Task Finish()
        {
            _timer.Stop();
            
            await _dataManager
                .AddDiagnostics(_application, "Requests", _requestId,
                    new DiagnosticsData(_step, new Dictionary<string, string>
                    {
                        [_name] = _timer.Elapsed.ToString()
                    })).ConfigureAwait(false);
        }
    }
}