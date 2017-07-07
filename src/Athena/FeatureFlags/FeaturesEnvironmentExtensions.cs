using Athena.Configuration;

namespace Athena.FeatureFlags
{
    public static class FeaturesEnvironmentExtensions
    {
        public static PartConfiguration<FeaturesSettings> Features(this AthenaBootstrapper bootstrapper)
        {
            return bootstrapper.Part<FeaturesSettings>();
        }
    }
}