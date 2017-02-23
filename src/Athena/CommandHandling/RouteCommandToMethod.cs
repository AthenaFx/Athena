using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Athena.Routing;

namespace Athena.CommandHandling
{
    public class RouteCommandToMethod : ToMethodRouter
    {
        private RouteCommandToMethod(IReadOnlyCollection<MethodInfo> availableMethods) : base(availableMethods)
        {
        }

        protected override MethodInfo Route(IDictionary<string, object> environment, IReadOnlyCollection<MethodInfo> availableMethods)
        {
            var command = environment.Get<object>("command");

            return command == null ? null : availableMethods.FirstOrDefault(x => x.GetParameters().Any(y => y.ParameterType == command.GetType()));
        }

        public static RouteCommandToMethod New(Func<MethodInfo, bool> filter)
        {
            var methods = AthenaApplications.ApplicationAssemblies
                .SelectMany(x => x.GetTypes())
                .SelectMany(x => x.GetMethods(BindingFlags.Instance | BindingFlags.Public))
                .Where(filter)
                .ToList();

            return new RouteCommandToMethod(new ReadOnlyCollection<MethodInfo>(methods));
        }
    }
}