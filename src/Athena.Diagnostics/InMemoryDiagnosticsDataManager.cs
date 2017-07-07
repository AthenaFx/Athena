using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Athena.Diagnostics
{
    public class InMemoryDiagnosticsDataManager : DiagnosticsDataManager
    {
        private static readonly
            IDictionary<string, ConcurrentDictionary<string, LurchTable<string, LurchList<DiagnosticsData>>>> Data =
                new ConcurrentDictionary<string,
                    ConcurrentDictionary<string, LurchTable<string, LurchList<DiagnosticsData>>>>();
        
        public Task AddDiagnostics(string application, string type, string step, DiagnosticsData data,
            IDictionary<string, object> environment)
        {
            var loweredApplication = (application ?? "").ToLower();
            var loweredType = (type ?? "").ToLower();
            var loweredStep = (step ?? "").ToLower();

            if (!Data.ContainsKey(loweredApplication))
                Data[loweredApplication] = new ConcurrentDictionary<string, LurchTable<string, LurchList<DiagnosticsData>>>();

            if (!Data[loweredApplication].ContainsKey(loweredType))
                Data[loweredApplication][loweredType] = new LurchTable<string, LurchList<DiagnosticsData>>(200);

            if (!Data[loweredApplication][loweredType].ContainsKey(loweredStep))
                Data[loweredApplication][loweredType][loweredStep] = new LurchList<DiagnosticsData>(50);

            Data[loweredApplication][loweredType][loweredStep].AddFirst(data);

            return Task.CompletedTask;
        }

        public Task<IEnumerable<string>> GetApplications()
        {
            return Task.FromResult<IEnumerable<string>>(Data.Keys.OrderBy(x => x));
        }

        public Task<IEnumerable<string>> GetTypesFor(string application)
        {
            var loweredApplication = (application ?? "").ToLower();

            ConcurrentDictionary<string, LurchTable<string, LurchList<DiagnosticsData>>> categoryData;

            return Task.FromResult(!Data.TryGetValue(loweredApplication, out categoryData) 
                ? Enumerable.Empty<string>() 
                : categoryData.Keys.OrderBy(x => x));
        }

        public Task<IEnumerable<string>> GetStepsFor(string application, string type, int numberOfSteps = 50)
        {
            var loweredApplication = (application ?? "").ToLower();
            var loweredType = (type ?? "").ToLower();

            ConcurrentDictionary<string, LurchTable<string, LurchList<DiagnosticsData>>> categoryData;

            if (!Data.TryGetValue(loweredApplication, out categoryData))
                return Task.FromResult(Enumerable.Empty<string>());

            LurchTable<string, LurchList<DiagnosticsData>> typeData;

            return Task.FromResult(!categoryData.TryGetValue(loweredType, out typeData) 
                ? Enumerable.Empty<string>() 
                : typeData.Keys.OrderBy(x => x).Take(numberOfSteps));
        }

        public Task<IReadOnlyDictionary<string, IEnumerable<KeyValuePair<string, string>>>> GetDataFor(
            string application, string type, string step)
        {
            var loweredApplication = (application ?? "").ToLower();
            var loweredType = (type ?? "").ToLower();
            var loweredStep = (step ?? "").ToLower();

            ConcurrentDictionary<string, LurchTable<string, LurchList<DiagnosticsData>>> categoryData;

            if (!Data.TryGetValue(loweredApplication, out categoryData))
                return Task.FromResult<IReadOnlyDictionary<string, IEnumerable<KeyValuePair<string, string>>>>
                    (new Dictionary<string, IEnumerable<KeyValuePair<string, string>>>());

            LurchTable<string, LurchList<DiagnosticsData>> typeData;

            if (!categoryData.TryGetValue(loweredType, out typeData))
                return Task.FromResult<IReadOnlyDictionary<string, IEnumerable<KeyValuePair<string, string>>>>
                    (new Dictionary<string, IEnumerable<KeyValuePair<string, string>>>());

            if (!typeData.ContainsKey(loweredStep))
                return Task.FromResult<IReadOnlyDictionary<string, IEnumerable<KeyValuePair<string, string>>>>
                    (new Dictionary<string, IEnumerable<KeyValuePair<string, string>>>());

            var result = typeData[loweredStep]
                .GroupBy(x => x.Key, x => x.Data)
                .ToDictionary(x => x.Key, x => x.SelectMany(y => y));

            return Task
                .FromResult<IReadOnlyDictionary<string, IEnumerable<KeyValuePair<string, string>>>>(result);
        }
    }
}