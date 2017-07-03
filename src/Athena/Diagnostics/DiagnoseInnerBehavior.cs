using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Logging;

namespace Athena.Diagnostics
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    
    public class DiagnoseInnerBehavior
    {
        private readonly AppFunc _next;
        private readonly string _nextItem;
        private readonly DiagnosticsDataManager _diagnosticsDataManager;
        
        public DiagnoseInnerBehavior(AppFunc next, string nextItem, DiagnosticsDataManager diagnosticsDataManager)
        {
            _next = next;
            _nextItem = nextItem;
            _diagnosticsDataManager = diagnosticsDataManager;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            Logger.Write(LogLevel.Debug, $"Starting to diagnose {_nextItem}");

            var context = _diagnosticsDataManager.OpenDiagnosticsTimerContext(environment, "Middlewares", _nextItem);

            await _next(environment).ConfigureAwait(false);

            await context.Finish().ConfigureAwait(false);

            Logger.Write(LogLevel.Debug, $"Diagnose of {_nextItem} finished");
        }
    }
}