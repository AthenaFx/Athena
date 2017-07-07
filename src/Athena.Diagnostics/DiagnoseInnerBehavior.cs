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
        private readonly DiagnosticsConfiguration _settings;
        
        public DiagnoseInnerBehavior(AppFunc next, string nextItem, DiagnosticsConfiguration settings)
        {
            _next = next;
            _nextItem = nextItem;
            _settings = settings;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            Logger.Write(LogLevel.Debug, $"Starting to diagnose {_nextItem}");

            var context = _settings.OpenDiagnosticsTimerContext(environment, "middlewares", _nextItem);

            await _next(environment).ConfigureAwait(false);

            await context.Finish().ConfigureAwait(false);

            Logger.Write(LogLevel.Debug, $"Diagnose of {_nextItem} finished");
        }
    }
}