using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena.Diagnostics
{
    public interface MetricsDataManager
    {
        Task ReportMetricsTotalValue(string application, string key, double value, DateTime at);
        Task ReportMetricsPerSecondValue(string application, string key, double value, DateTime at);
        Task ReportMetricsApdexValue(string application, string key, double value, DateTime at, double tolerable);
        Task<double> GetAverageFor(string application, string key);
        Task<IReadOnlyCollection<string>> GetKeys(string application);
    }
}