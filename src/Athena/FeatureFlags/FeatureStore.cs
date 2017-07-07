using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena.FeatureFlags
{
    public interface FeatureStore
    {
        bool IsOn(string feature, IDictionary<string, object> environment);
        Task Initialize(IReadOnlyDictionary<string, FeatureFlagCalculator> defaultCalculators);
    }
}