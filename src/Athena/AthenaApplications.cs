using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyModel;

namespace Athena
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public sealed class AthenaApplications : AthenaContext, AthenaBootstrapper
    {
        private static readonly IDictionary<string, AppFunc> Applications = new ConcurrentDictionary<string, AppFunc>();
        private readonly IEnumerable<AthenaPlugin> _plugins;

        public static IReadOnlyCollection<Assembly> ApplicationAssemblies { get; private set; }

        private AthenaApplications(IEnumerable<AthenaPlugin> plugins)
        {
            _plugins = plugins;
        }

        public void DefineApplication(string name, AppFunc app, bool overwrite = true)
        {
            if (overwrite || !Applications.ContainsKey(name))
                Applications[name] = app;
        }

        public async Task Execute(string application, IDictionary<string, object> environment)
        {
            using (environment.EnterApplication(this, application))
                await Applications[application](environment);
        }

        public Task ShutDown()
        {
            return Task.WhenAll(_plugins.Select(x => x.TearDown(this)));
        }

        public static async Task<AthenaContext> Bootsrap(Action<AthenaBootstrapper> bootstrap,
            params Assembly[] applicationAssemblies)
        {
            ApplicationAssemblies = applicationAssemblies;

            var pluginType = typeof(AthenaPlugin);

            var plugins = GetReferencingAssemblies(pluginType.GetTypeInfo().Assembly.GetName().Name)
                .SelectMany(x => x.GetTypes()
                    .Where(y =>
                    {
                        var typeInfo = y.GetTypeInfo();

                        return pluginType.IsAssignableFrom(y) && !typeInfo.IsAbstract && !typeInfo.IsInterface;
                    }))
                .Select(Activator.CreateInstance)
                .OfType<AthenaPlugin>()
                .ToList();
            
            var context = new AthenaApplications(plugins);

            await Task.WhenAll(plugins.Select(x => x.Bootstrap(context)));

            bootstrap(context);
            
            return context;
        }
        
        public static Task<AthenaContext> Bootsrap(params Assembly[] applicationAssemblies)
        {
            return Bootsrap(x => { }, applicationAssemblies);
        }

        private static IEnumerable<Assembly> GetReferencingAssemblies(string assemblyName)
        {
            var dependencies = DependencyContext.Default.RuntimeLibraries;

            return (from library in dependencies
                where IsCandidateLibrary(library, assemblyName)
                select Assembly.Load(new AssemblyName(library.Name))).ToList();
        }

        private static bool IsCandidateLibrary(Library library, string assemblyName)
        {
            return library.Name == assemblyName
                   || library.Dependencies.Any(d => d.Name.StartsWith(assemblyName));
        }
    }
}