﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena.Diagnostics
{
    public interface DiagnosticsDataManager
    {
        Task AddDiagnostics(string application, string type, string step, DiagnosticsData data,
            IDictionary<string, object> environment);
        
        Task<IEnumerable<string>> GetApplications();
        
        Task<IEnumerable<string>> GetTypesFor(string application);
        
        Task<IEnumerable<string>> GetStepsFor(string application, string type, int numberOfSteps = 50);
        
        Task<IReadOnlyDictionary<string, IEnumerable<KeyValuePair<string, string>>>> 
            GetDataFor(string application, string type, string step);
    }
}