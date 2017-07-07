using Athena.Configuration;

namespace Athena.FeatureFlags
{
    public class FeatureFlagComponent : AthenaComponent
    {
        public AthenaBootstrapper Configure(AthenaBootstrapper bootstrapper)
        {
            return bootstrapper
                .Features()
                .On<BootstrapCompleted>(async (conf, evnt, context) =>
                {
                    await conf
                        .FeatureStore
                        .Initialize(conf.GetDefaultFeatures())
                        .ConfigureAwait(false);
                });
        }
    }
}