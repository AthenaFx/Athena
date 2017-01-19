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

    public sealed class AthenaApplications : AthenaContext
    {
        private static readonly IDictionary<string, AppFunc> Applications = new ConcurrentDictionary<string, AppFunc>();

        private AthenaApplications()
        {

        }

        public void DefineApplication(string name, AppFunc app)
        {
            Applications[name] = app;
        }

        public async Task Execute(string application, IDictionary<string, object> environment)
        {
            using(environment.EnterApplication(application))
                await Applications[application](environment);
        }

        public static async Task<AthenaContext> Bootsrap()
        {
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

            var context = new AthenaApplications();

            await Task.WhenAll(plugins.Select(x => x.Start(context)));

            return context;
        }

        private static IEnumerable<Assembly> GetReferencingAssemblies(string assemblyName)
        {
            var dependencies = DependencyContext.Default.RuntimeLibraries;

            return (from library in dependencies where IsCandidateLibrary(library, assemblyName)
                select Assembly.Load(new AssemblyName(library.Name))).ToList();
        }

        private static bool IsCandidateLibrary(Library library, string assemblyName)
        {
            return library.Name == assemblyName
                   || library.Dependencies.Any(d => d.Name.StartsWith(assemblyName));
        }
    }
}