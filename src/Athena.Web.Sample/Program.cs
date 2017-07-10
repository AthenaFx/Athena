using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Athena.CommandHandling;
using Athena.Configuration;
using Athena.Diagnostics;
using Athena.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Owin;
using Microsoft.Extensions.Logging;
using LogLevel = Athena.Logging.LogLevel;

namespace Athena.Web.Sample
{
    public class Program
    {
        public static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();

        private static async Task MainAsync()
        {
            var athenaContext = await AthenaApplications
                .From("dev", typeof(Program).GetTypeInfo().Assembly)
                .LogToConsole(LogLevel.Debug)
                .EnableDiagnostics()
                .UsingWebApplication()
                .EnableCommandSender()
                .Build();
            
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureLogging(x => x.AddConsole())
                .Configure(app =>
                {
                    app.Run(context => athenaContext.Execute("web", new OwinEnvironment(context)));
                })
                .Build();

            host.Run();

            await athenaContext.ShutDown();
        }
    }
}