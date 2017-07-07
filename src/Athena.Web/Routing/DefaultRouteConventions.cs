using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Athena.Logging;

namespace Athena.Web.Routing
{
    public static class DefaultRouteConventions
    {
        private static readonly IReadOnlyCollection<string> AvailableMethodNames = new ReadOnlyCollection<string>(new List<string>
        {
            "Get",
            "Post",
            "Put",
            "Delete",
            "Patch"
        });
        
        public static IReadOnlyCollection<Route> BuildRoutes(params Assembly[] assemblies)
        {
            return BuildRoutes(x => x, x => true, new List<string>(), assemblies);
        }

        public static IReadOnlyCollection<Route> BuildRoutes(Func<Type, bool> filter, params Assembly[] assemblies)
        {
            return BuildRoutes(x => x, filter, new List<string>(), assemblies);
        }
        
        public static IReadOnlyCollection<Route> BuildRoutes(Func<string, string> alterPattern, 
            params Assembly[] assemblies)
        {
            return BuildRoutes(alterPattern, x => true, new List<string>(), assemblies);
        }

        public static IReadOnlyCollection<Route> BuildRoutes(Func<string, string> alterPattern, Func<Type, bool> filter, 
            IReadOnlyCollection<string> extraUrlParameters, params Assembly[] assemblies)
        {
            var availableAssemblies = assemblies.ToList();

            var availableMethods = availableAssemblies
                .SelectMany(x => x.GetTypes())
                .Where(filter)
                .SelectMany(x => x.GetTypeInfo().GetMethods())
                .Where(x => AvailableMethodNames.Contains(x.Name))
                .ToList();

            var result = new List<Route>();

            foreach (var availableMethod in availableMethods)
            {
                var routeParts = new List<string>();

                if (!string.IsNullOrEmpty(availableMethod.DeclaringType.Namespace))
                {
                    var namespacePart = availableMethod.DeclaringType.Namespace.Split('.').Last();

                    if(!namespacePart.Equals("Home", StringComparison.OrdinalIgnoreCase))
                        routeParts.Add(namespacePart.ToLower().Pluralize());
                }

                var hasSlug = availableMethod
                    .GetParameters()
                    .Any(x => x.Name.Equals("Slug", StringComparison.OrdinalIgnoreCase)
                              || x.ParameterType
                                  .GetTypeInfo()
                                  .GetProperties()
                                  .Any(y => y.Name.Equals("Slug", StringComparison.OrdinalIgnoreCase)));

                if(hasSlug)
                    routeParts.Add("{slug}");

                if (!availableMethod.DeclaringType.Name.Equals("Index", StringComparison.OrdinalIgnoreCase)
                    && !availableMethod.DeclaringType.Name.Equals("Details", StringComparison.OrdinalIgnoreCase))
                {
                    routeParts.Add(availableMethod.DeclaringType.Name.ToLower());
                }

                var hasId = availableMethod
                    .GetParameters()
                    .Any(x => x.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)
                              || x.ParameterType
                                  .GetTypeInfo()
                                  .GetProperties()
                                  .Any(y => y.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)));

                if(hasId)
                    routeParts.Add("{id}");

                foreach (var extraUrlParameter in extraUrlParameters)
                {
                    var hasParameter = availableMethod
                        .GetParameters()
                        .Any(x => x.Name.Equals(extraUrlParameter, StringComparison.OrdinalIgnoreCase)
                                  || x.ParameterType
                                      .GetTypeInfo()
                                      .GetProperties()
                                      .Any(y => y.Name.Equals(extraUrlParameter, StringComparison.OrdinalIgnoreCase)));
                    
                    if(hasParameter)
                        routeParts.Add($"{{{extraUrlParameter.ToLower()}}}");
                }

                var pattern = alterPattern(string.Join("/", routeParts));

                Logger.Write(LogLevel.Debug,
                    $"Adding route with pattern {pattern} for {availableMethod.DeclaringType.Namespace}.{availableMethod.DeclaringType.Name}.{availableMethod.Name}()");
                
                result.Add(new Route(pattern, availableMethod, new List<string>
                {
                    availableMethod.Name.ToUpper()
                }));
            }

            return result;
        }
    }
}