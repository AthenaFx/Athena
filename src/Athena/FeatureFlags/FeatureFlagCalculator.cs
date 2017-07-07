using System.Collections.Generic;

namespace Athena.FeatureFlags
{
    public interface FeatureFlagCalculator
    {
        bool IsOn(IDictionary<string, object> environment);
    }
}