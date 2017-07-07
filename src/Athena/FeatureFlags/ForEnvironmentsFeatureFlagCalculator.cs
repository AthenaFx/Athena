using System.Collections.Generic;
using System.Linq;

namespace Athena.FeatureFlags
{
    public class ForEnvironmentsFeatureFlagCalculator : FeatureFlagCalculator
    {
        public ForEnvironmentsFeatureFlagCalculator(params string[] environments)
        {
            Environments = environments;
        }
        
        public IReadOnlyCollection<string> Environments { get; }
        
        public bool IsOn(IDictionary<string, object> environment)
        {
            var context = environment.GetAthenaContext();
            
            return Environments.Contains(context?.Environment ?? "");
        }
    }
}