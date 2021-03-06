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
        private readonly IReadOnlyDictionary<string, PartConfiguration> _configurations;
        
        private ApplicationsContext(IReadOnlyCollection<Assembly> applicationAssemblies, string applicationName, 
            string environment, IReadOnlyDictionary<string, AppFunc> applications, 
            IReadOnlyDictionary<string, PartConfiguration> configurations)
        {
            ApplicationAssemblies = applicationAssemblies;
            ApplicationName = applicationName;
            _applications = applications;
            _configurations = configurations;
            Environment = environment;
        }

        public static async Task<ApplicationsContext> From(IReadOnlyCollection<Assembly> applicationAssemblies, 
            string applicationName, string environment, IReadOnlyDictionary<string, AppFunc> applications, 
            IReadOnlyDictionary<string, PartConfiguration> configurations)
        {
            var context = new ApplicationsContext(applicationAssemblies, applicationName, environment, applications,
                configurations);

            await Task.WhenAll(configurations.Select(x => x.Value.Startup(context))).ConfigureAwait(false);

            return context;
        }

        public IReadOnlyCollection<Assembly> ApplicationAssemblies { get; }
        public string ApplicationName { get; }
        public string Environment { get; }

        public TSetting GetSetting<TSetting>(string key = null) where TSetting : class
        {
            return GetSetting(typeof(TSetting), key) as TSetting;
        }

        public object GetSetting(Type type, string key = null)
        {
            if (string.IsNullOrEmpty(key))
                key = type.FullName;
            
            Logger.Write(LogLevel.Debug, $"Getting settings {type} with key {key}");

            return _configurations.ContainsKey(key) ? _configurations[key].GetPart() : null;
        }

        public async Task Execute(string application, IDictionary<string, object> environment)
        {
            Logger.Write(LogLevel.Debug, $"Executing application {application}");

            using (environment.EnterApplication(this, application))
            {
                var timer = Stopwatch.StartNew();
                
                await _applications[application](environment).ConfigureAwait(false);
                
                timer.Stop();

                EventPublishing.Publish(new ApplicationExecutedRequest(application,
                    environment.GetRequestId(), timer.Elapsed, DateTime.UtcNow), environment);
            }
            
            Logger.Write(LogLevel.Debug, $"Application executed {application}");
        }

        public async Task ShutDown()
        {
            var environment = new Dictionary<string, object>();
            
            using (environment.EnterApplication(this, "shutdown"))
            {
                Logger.Write(LogLevel.Debug, "Starting shutdown of application");
                
                EventPublishing.Publish(new ShutdownStarted(ApplicationName, Environment), environment);

                await Task.WhenAll(_configurations.Select(x => x.Value.Shutdown(this))).ConfigureAwait(false);
            
                Logger.Write(LogLevel.Debug, "Applications shut down");
            }
        }
    }
}