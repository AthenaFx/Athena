using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Athena.Logging;

namespace Athena.Web.ModelBinding.BindingSources
{
    public static class BindingSourcesExtensions
    {
        public static async Task<bool> ContainsKey(this IReadOnlyCollection<BindingSource> bindingSources, string key, IDictionary<string, object> environment)
        {
            var result = (await GetSourcesContainingKey(bindingSources, key.ToLower(), environment).ConfigureAwait(false)).Any();

            Logger.Write(LogLevel.Debug, $"Searched for binding key: {key} with result: Success = {result}.");

            return result;
        }

        public static async Task<object> Get(this IReadOnlyCollection<BindingSource> bindingSources, string key, IDictionary<string, object> environment)
        {
            Logger.Write(LogLevel.Debug, $"Searching for binding key: {key}");

            var matchingSources = (await GetSourcesContainingKey(bindingSources, key.ToLower(), environment).ConfigureAwait(false)).ToList();

            if (!matchingSources.Any())
            {
                Logger.Write(LogLevel.Debug, $"Failed to find any matching source for binding key: {key}");

                return null;
            }

            var bindingSource = matchingSources.First();

            var result = (await bindingSource.GetValues(environment).ConfigureAwait(false))[key.ToLower()];

            Logger.Write(LogLevel.Debug, $"Binding key: {key} with value: {result ?? "null"} using source: {bindingSource}");

            return result;
        }

        private static async Task<IEnumerable<BindingSource>> GetSourcesContainingKey(IEnumerable<BindingSource> bindingSources, string key, IDictionary<string, object> environment)
        {
            var results = new List<BindingSource>();

            foreach (var bindingSource in bindingSources)
            {
                if ((await bindingSource.GetValues(environment).ConfigureAwait(false)).ContainsKey(key.ToLower()))
                    results.Add(bindingSource);
            }

            Logger.Write(LogLevel.Debug,
                $"Found {results.Count} sources containing key: {key}. \"{string.Join(", ", results.Select(x => x.GetType().Name))}\"");

            return results;
        }
    }
}