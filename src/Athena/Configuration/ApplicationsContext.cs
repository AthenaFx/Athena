using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Athena.Configuration
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    
    internal class ApplicationsContext : AthenaContext
    {
        private readonly IReadOnlyDictionary<string, AppFunc> _applications;
        private readonly IReadOnlyCollection<AthenaPlugin> _plugins;
        
        public ApplicationsContext(IReadOnlyCollection<Assembly> applicationAssemblies, string applicationName, 
            string environment, IReadOnlyDictionary<string, AppFunc> applications, 
            IReadOnlyCollection<AthenaPlugin> plugins)
        {
            ApplicationAssemblies = applicationAssemblies;
            ApplicationName = applicationName;
            _applications = applications;
            _plugins = plugins;
            Environment = environment;
        }

        public IReadOnlyCollection<Assembly> ApplicationAssemblies { get; }
        public string ApplicationName { get; }
        public string Environment { get; }

        public async Task Execute(string application, IDictionary<string, object> environment)
        {
            using (environment.EnterApplication(this, application))
                await _applications[application](environment).ConfigureAwait(false);
        }

        public Task ShutDown()
        {
            return Task.WhenAll(_plugins.Select(x => x.TearDown(this)));
        }
    }
}