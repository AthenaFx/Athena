namespace Athena.Diagnostics
{
    public class DiagnosticsConfiguration
    {
        public DiagnosticsConfiguration()
        {
            DataManager = new InMemoryDiagnosticsDataManager();
        }

        public DiagnosticsDataManager DataManager { get; private set; }

        public DiagnosticsConfiguration UsingDataManager(DiagnosticsDataManager dataManager)
        {
            DataManager = dataManager;

            return this;
        }
    }
}