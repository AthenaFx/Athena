using Athena.Configuration;

namespace Athena.Web.Client
{
    public static class BootstrapperExtensions
    {
        public static PartConfiguration<ClientWebApplicationSettings> ClientWebApplication(
            this AthenaBootstrapper bootstrapper, string name = "client_default")
        {
            var key = $"_client_web_application_{name}";
            
            return bootstrapper
                .UsingWebApplication($"web_{name}")
                .Configure(x => x.ModifyApplication(builder => builder
                    .Remove("HandleTransactions")
                    .Remove("SupplyMetaData")
                    .Remove("ValidateParameters")
                    .Remove("ExecuteResource")))
                .Child<ClientWebApplicationSettings>(key);
        }
    }

    public class ClientWebApplicationSettings
    {
        public string RenderFile { get; private set; } = "~/Index.html";

        public ClientWebApplicationSettings UsingFile(string file)
        {
            RenderFile = file;

            return this;
        }
    }
}