using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena.Diagnostics
{
    public interface MetricsDataManager
    {
        Task ReportMetricsValue(string application, string key, double value, DateTime at);
        Task<double> GetAverageFor(string application, string key);
        Task<IReadOnlyCollection<string>> GetKeys(string application);
    }
}