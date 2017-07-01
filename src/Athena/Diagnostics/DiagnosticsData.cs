using System.Collections.Generic;

namespace Athena.Diagnostics
{
    public class DiagnosticsData
    {
        public DiagnosticsData(string key, IReadOnlyDictionary<string, string> data)
        {
            Key = key ?? "";
            Data = data ?? new Dictionary<string, string>();
        }

        public string Key { get; }
        public IReadOnlyDictionary<string, string> Data { get; }
    }
}