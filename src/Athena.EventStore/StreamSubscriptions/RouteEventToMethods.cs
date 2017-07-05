using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Athena.EventStore.Serialization;
using Athena.Routing;

namespace Athena.EventStore.StreamSubscriptions
{
    public class RouteEventToMethods : ToMultipleMethodsRouter
    {
        private readonly Func<Type, IDictionary<string, object>, object> _createInstance;
        
        private RouteEventToMethods(IReadOnlyCollection<MethodInfo> availableMethods, 
            Func<Type, IDictionary<string, object>, object> createInstance) : base(availableMethods)
        {
            _createInstance = createInstance;
        }

        protected override IEnumerable<MethodInfo> Route(IDictionary<string, object> environment, 
            IReadOnlyCollection<MethodInfo> availableMethods)
        {
            var evnt = environment.Get<DeSerializationResult>("event");

            if (evnt == null)
                return Enumerable.Empty<MethodInfo>();

            return availableMethods
                .Where(x => x.GetParameters().First().ParameterType == evnt.Data.GetType())
                .ToList();
        }

        protected override object CreateInstance(Type type, IDictionary<string, object> environment)
        {
            return _createInstance(type, environment);
        }

        protected override KeyValuePair<string, string> GetRouteFor(MethodInfo methodInfo)
        {
            var eventType = methodInfo.GetParameters().First().ParameterType;
            
            return new KeyValuePair<string, string>(eventType.ToString(),
                $"{methodInfo.DeclaringType.Namespace}.{methodInfo.DeclaringType.Name}.{methodInfo.Name}()");
        }

        public static RouteEventToMethods New(Func<MethodInfo, bool> filter, IEnumerable<Assembly> assemblies,
            Func<Type, IDictionary<string, object>, object> createInstance)
        {
            var methods = assemblies
                .SelectMany(x => x.GetTypes())
                .SelectMany(x => x.GetMethods(BindingFlags.Instance | BindingFlags.Public))
                .Where(filter)
                .ToList();

            return new RouteEventToMethods(new ReadOnlyCollection<MethodInfo>(methods), createInstance);
        }
    }
}