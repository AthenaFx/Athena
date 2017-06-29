using System;
using System.Collections.Generic;

namespace Athena.Diagnostics
{
    public class DiagnosticsData
    {
        public DiagnosticsData(string key, IReadOnlyDictionary<string, DiagnosticsValue> data)
        {
            Key = key ?? "";
            Data = data ?? new Dictionary<string, DiagnosticsValue>();
        }

        public string Key { get; private set; }
        public IReadOnlyDictionary<string, DiagnosticsValue> Data { get; }
    }
}