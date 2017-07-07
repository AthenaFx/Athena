using System.Collections.Generic;

namespace Athena.FeatureFlags
{
    public class OnFeatureFlagCalculator : FeatureFlagCalculator
    {
        public bool IsOn(IDictionary<string, object> environment)
        {
            return true;
        }
    }
}