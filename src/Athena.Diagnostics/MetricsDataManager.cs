using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena.Diagnostics
{
    public interface MetricsDataManager
    {
        Task ReportMetricsTotalValue(string application, string key, double value, DateTime at,
            IDictionary<string, object> environment);
        Task ReportMetricsPerSecondValue(string application, string key, double value, DateTime at,
            IDictionary<string, object> environment);
        Task ReportMetricsApdexValue(string application, string key, double value, DateTime at, double tolerable,
            IDictionary<string, object> environment);
        Task<double> GetAverageFor(string application, string key);
        Task<IReadOnlyCollection<string>> GetKeys(string application);
    }
}