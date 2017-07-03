using Athena.Configuration;
using Athena.Logging;

namespace Athena.ApplicationTimeouts
{
    public static class Timeouts
    {
        public static PartConfiguration<TimeoutManager> EnableTimeouts(this AthenaBootstrapper bootstrapper)
        {
            Logger.Write(LogLevel.Debug, "Enabling timeouts");

            return bootstrapper
                .Part<TimeoutManager>()
                .OnStartup((timeoutManager, context) => timeoutManager.Start())
                .OnShutdown((timeoutManager, context) => timeoutManager.Stop());
        }
    }
}