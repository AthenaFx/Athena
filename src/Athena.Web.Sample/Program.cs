﻿using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Athena.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Owin;
using Microsoft.Extensions.Logging;

namespace Athena.Web.Sample
{
    public class Program
    {
        public static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();

        private static async Task MainAsync()
        {
            var athenaContext = await AthenaApplications
                .From("local", typeof(Program).GetTypeInfo().Assembly)
                .UsingPlugin(new WebAppPlugin())
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

            athenaContext.ShutDown().Wait();
        }
    }
}