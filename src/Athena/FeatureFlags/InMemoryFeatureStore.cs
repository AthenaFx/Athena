using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Athena.FeatureFlags
{
    public class InMemoryFeatureStore : FeatureStore
    {
        private readonly ConcurrentDictionary<string, FeatureFlagCalculator> _features 
            = new ConcurrentDictionary<string, FeatureFlagCalculator>();
        
        public bool IsOn(string feature, IDictionary<string, object> environment)
        {
            var parts = new Stack<string>(feature.Split('-'));
            var enabled = false;

            while (parts.Count > 0)
            {
                var currentFeature = string.Join("-", parts.Reverse());

                FeatureFlagCalculator calculator;
                if (!_features.TryGetValue(currentFeature, out calculator))
                {
                    parts.Pop();
                    continue;
                }
                
                var available = calculator.IsOn(environment);

                if (!available)
                    return false;

                enabled = true;

                parts.Pop();
            }

            return enabled;
        }

        public Task Initialize(IReadOnlyDictionary<string, FeatureFlagCalculator> defaultCalculators)
        {
            foreach (var calculator in defaultCalculators)
                _features.GetOrAdd(calculator.Key, calculator.Value);
            
            return Task.CompletedTask;
        }
    }
}