namespace Athena.Configuration
{
    public class BootstrapStarted : SetupEvent
    {
        public BootstrapStarted(string applicationName, string environment)
        {
            ApplicationName = applicationName;
            Environment = environment;
        }

        public string ApplicationName { get; }
        public string Environment { get; }
    }
}