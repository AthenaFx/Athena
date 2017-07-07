using System.Collections.Generic;
using System.Linq;

namespace Athena.FeatureFlags
{
    public class FeaturesSettings
    {
        private readonly IDictionary<string, FeatureFlagCalculator> _defaultFeatureCalculators 
            = new Dictionary<string, FeatureFlagCalculator>();
        
        public FeatureStore FeatureStore { get; private set; } = new InMemoryFeatureStore();

        public FeaturesSettings UsingStore(FeatureStore store)
        {
            FeatureStore = store;

            return this;
        }

        public FeaturesSettings WithDefaultFeature(string feature, FeatureFlagCalculator calculator)
        {
            _defaultFeatureCalculators[feature] = calculator;

            return this;
        }

        internal IReadOnlyDictionary<string, FeatureFlagCalculator> GetDefaultFeatures()
        {
            return _defaultFeatureCalculators.ToDictionary(x => x.Key, x => x.Value);
        }
    }
}