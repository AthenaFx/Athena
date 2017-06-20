using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Athena.EventStore.Serialization;
using Athena.Routing;

namespace Athena.EventStore.StreamSubscriptions
{
    public class RouteEventToMethod : ToMethodRouter
    {
        private RouteEventToMethod(IReadOnlyCollection<MethodInfo> availableMethods) : base(availableMethods)
        {
        }

        protected override MethodInfo Route(IDictionary<string, object> environment, 
            IReadOnlyCollection<MethodInfo> availableMethods)
        {
            var evnt = environment.Get<DeSerializationResult>("event");

            return evnt == null 
                ? null 
                : availableMethods.FirstOrDefault(x => x.GetParameters()
                    .Any(y => y.ParameterType == evnt.Data.GetType()));
        }

        public static RouteEventToMethod New(Func<MethodInfo, bool> filter)
        {
            var methods = AthenaApplications.ApplicationAssemblies
                .SelectMany(x => x.GetTypes())
                .SelectMany(x => x.GetMethods(BindingFlags.Instance | BindingFlags.Public))
                .Where(filter)
                .ToList();

            return new RouteEventToMethod(new ReadOnlyCollection<MethodInfo>(methods));
        }
    }
}