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
        private readonly Func<Type, IDictionary<string, object>, object> _createInstance;
        
        private RouteEventToMethod(IReadOnlyCollection<MethodInfo> availableMethods, 
            Func<Type, IDictionary<string, object>, object> createInstance) : base(availableMethods)
        {
            _createInstance = createInstance;
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

        protected override object CreateInstance(Type type, IDictionary<string, object> environment)
        {
            return _createInstance(type, environment);
        }

        public static RouteEventToMethod New(Func<MethodInfo, bool> filter, IEnumerable<Assembly> assemblies,
            Func<Type, IDictionary<string, object>, object> createInstance)
        {
            var methods = assemblies
                .SelectMany(x => x.GetTypes())
                .SelectMany(x => x.GetMethods(BindingFlags.Instance | BindingFlags.Public))
                .Where(filter)
                .ToList();

            return new RouteEventToMethod(new ReadOnlyCollection<MethodInfo>(methods), createInstance);
        }
    }
}