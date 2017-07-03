﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Athena.Logging;
using Athena.PubSub;

namespace Athena.Configuration
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    
    internal class ApplicationsContext : AthenaContext
    {
        private readonly IReadOnlyDictionary<string, AppFunc> _applications;

        private IReadOnlyCollection<Func<AthenaContext, Task>> _shutdownHandlers;
        
        private IReadOnlyDictionary<string, object> _settings;
        
        private ApplicationsContext(IReadOnlyCollection<Assembly> applicationAssemblies, string applicationName, 
            string environment, IReadOnlyDictionary<string, AppFunc> applications)
        {
            ApplicationAssemblies = applicationAssemblies;
            ApplicationName = applicationName;
            _applications = applications;
            Environment = environment;
        }

        public static async Task<ApplicationsContext> From(IReadOnlyCollection<Assembly> applicationAssemblies, 
            string applicationName, string environment, IReadOnlyDictionary<string, AppFunc> applications, 
            IReadOnlyDictionary<string, PartConfiguration> configurations)
        {
            var context = new ApplicationsContext(applicationAssemblies, applicationName, environment, applications);

            context._shutdownHandlers = await Task.WhenAll(configurations
                .Select(x => x.Value.RunStartup(context))).ConfigureAwait(false);

            context._settings = configurations.ToDictionary(x => x.Key, x => x.Value.GetPart());

            return context;
        }

        public IReadOnlyCollection<Assembly> ApplicationAssemblies { get; }
        public string ApplicationName { get; }
        public string Environment { get; }

        public TSetting GetSetting<TSetting>(string key = null) where TSetting : class
        {
            if (string.IsNullOrEmpty(key))
                key = typeof(TSetting).AssemblyQualifiedName;
            
            Logger.Write(LogLevel.Debug, $"Getting settings {typeof(TSetting)} with key {key}");

            return _settings.ContainsKey(key) ? _settings[key] as TSetting : null;
        }

        public async Task Execute(string application, IDictionary<string, object> environment)
        {
            Logger.Write(LogLevel.Debug, $"Executing application {application}");

            using (environment.EnterApplication(this, application))
            {
                var timer = Stopwatch.StartNew();
                
                await _applications[application](environment).ConfigureAwait(false);
                
                timer.Stop();

                await EventPublishing.Publish(new ApplicationExecutedRequest(application,
                    environment.GetRequestId(), timer.Elapsed, DateTime.UtcNow))
                    .ConfigureAwait(false);
            }
            
            Logger.Write(LogLevel.Debug, $"Application executed {application}");
        }

        public async Task ShutDown()
        {
            Logger.Write(LogLevel.Debug, "Starting shutdown of application");
            
            await EventPublishing.Publish(new ShutdownStarted(ApplicationName, Environment));

            await Task.WhenAll(_shutdownHandlers.Select(x => x(this))).ConfigureAwait(false);
            
            Logger.Write(LogLevel.Debug, "Applications shut down");
        }
    }
}