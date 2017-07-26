namespace Athena.Web.Client
{
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