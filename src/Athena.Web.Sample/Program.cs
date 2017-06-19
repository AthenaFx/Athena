using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Owin;
using Microsoft.Extensions.Logging;

namespace Athena.Web.Sample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var athenaApplication = AthenaApplications
                .Bootsrap(typeof(Program).GetTypeInfo().Assembly)
                .Result;
            
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureLogging(x => x.AddConsole())
                .Configure(app =>
                {
                    app.Run(async context =>
                    {
                        var owinEnvironment = new OwinEnvironment(context);

                        await athenaApplication.Execute("web", owinEnvironment);
                    });
                })
                .Build();

            host.Run();

            athenaApplication.ShutDown().Wait();
        }
    }
}