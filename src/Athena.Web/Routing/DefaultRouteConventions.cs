using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

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
            var availableAssemblies = assemblies.ToList();

            var availableMethods = availableAssemblies
                .SelectMany(x => x.GetTypes())
                .SelectMany(x => x.GetMethods())
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
                              || x.GetType()
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
                              || x.GetType()
                                  .GetProperties()
                                  .Any(y => y.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)));

                if(hasId)
                    routeParts.Add("{id}");

                result.Add(new Route(string.Join("/", routeParts), availableMethod, new List<string>
                {
                    availableMethod.Name.ToUpper()
                }));
            }

            return result;
        }
    }
}