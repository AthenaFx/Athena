using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

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
            var timer = Stopwatch.StartNew();

            await _next(environment).ConfigureAwait(false);
            
            timer.Stop();

            await _diagnosticsDataManager.AddDiagnostics(environment.GetCurrentApplication(),
                DiagnosticsTypes.MiddlewareExecution, environment.GetRequestId(), 
                new DiagnosticsData($"MiddleWare-{_nextItem}-Executed", new Dictionary<string, string>
                {
                    ["Middleware"] = _nextItem,
                    ["ExecutionTime"] = timer.Elapsed.ToString()
                }));
        }
    }
}