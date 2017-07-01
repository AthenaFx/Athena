using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Athena.PubSub;

namespace Athena.Configuration
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    
    internal class ApplicationsContext : AthenaContext
    {
        private readonly IReadOnlyDictionary<string, AppFunc> _applications;

        private readonly IReadOnlyCollection<Tuple<Type, Func<object, bool>, Func<object, AthenaContext, Task>>>
            _shutdownHandlers;
        
        private readonly IReadOnlyDictionary<string, object> _settings;
        
        public ApplicationsContext(IReadOnlyCollection<Assembly> applicationAssemblies, string applicationName, 
            string environment, IReadOnlyDictionary<string, AppFunc> applications, 
            IReadOnlyCollection<Tuple<Type, Func<object, bool>, Func<object, AthenaContext, Task>>> shutdownHandlers, 
            IReadOnlyDictionary<string, object> settings)
        {
            ApplicationAssemblies = applicationAssemblies;
            ApplicationName = applicationName;
            _applications = applications;
            _shutdownHandlers = shutdownHandlers;
            _settings = settings;
            Environment = environment;
        }

        public IReadOnlyCollection<Assembly> ApplicationAssemblies { get; }
        public string ApplicationName { get; }
        public string Environment { get; }

        public TSetting GetSetting<TSetting>(string key = null) where TSetting : class
        {
            if (string.IsNullOrEmpty(key))
                key = typeof(TSetting).AssemblyQualifiedName;

            return _settings.ContainsKey(key) ? _settings[key] as TSetting : null;
        }

        public async Task Execute(string application, IDictionary<string, object> environment)
        {
            using (environment.EnterApplication(this, application))
                await _applications[application](environment).ConfigureAwait(false);
        }

        public Task ShutDown()
        {
            return Done(new ShutdownStarted(ApplicationName, Environment));
        }
        
        private async Task Done(ShutdownEvent evnt)
        {
            foreach (var type in evnt.GetType().GetParentTypesFor())
            {
                var subscriptions = _shutdownHandlers.Where(x => x.Item1 == type && x.Item2(evnt)).ToList();

                await Task.WhenAll(subscriptions
                    .Select(x => x.Item3(evnt, this)))
                    .ConfigureAwait(false);
            }

            await this.Publish(evnt).ConfigureAwait(false);
        }
    }
}