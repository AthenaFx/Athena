namespace Athena.Configuration
{
    public interface AthenaComponent
    {
        AthenaBootstrapper Configure(AthenaBootstrapper bootstrapper);
    }
}