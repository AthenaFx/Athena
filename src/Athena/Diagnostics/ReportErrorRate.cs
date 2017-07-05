using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena.Diagnostics
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    
    public class ReportErrorRate
    {
        private readonly AppFunc _next;
        private readonly Func<IDictionary<string, object>, bool> _hasError;
        private readonly MetricsDataManager _dataManager;
        
        public ReportErrorRate(AppFunc next, Func<IDictionary<string, object>, bool> hasError, 
            MetricsDataManager dataManager)
        {
            _next = next;
            _hasError = hasError;
            _dataManager = dataManager;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            try
            {
                await _next(environment).ConfigureAwait(false);

                var hasError = _hasError(environment);

                await _dataManager.ReportMetricsTotalValue(environment.GetCurrentApplication(),
                    "errorrate", hasError ? 1 : 0, DateTime.UtcNow).ConfigureAwait(false);
            }
            catch (Exception)
            {
                await _dataManager.ReportMetricsTotalValue(environment.GetCurrentApplication(),
                    "errorrate", 1, DateTime.UtcNow).ConfigureAwait(false);
                
                throw;
            }
        }
    }
}