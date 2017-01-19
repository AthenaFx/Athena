using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Owin;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Athena.Web.Sample
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            var athenaApplication = AthenaApplications.Bootsrap().Result;

            app.Run(async context =>
            {
                var owinEnvironment = new OwinEnvironment(context);

                await athenaApplication.Execute("web", owinEnvironment);
            });
        }
    }
}