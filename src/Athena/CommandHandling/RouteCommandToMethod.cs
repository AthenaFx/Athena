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
        private readonly Func<Type, IDictionary<string, object>, object> _createInstance;
        
        private RouteCommandToMethod(IReadOnlyCollection<MethodInfo> availableMethods, 
            Func<Type, IDictionary<string, object>, object> createInstance) : base(availableMethods)
        {
            _createInstance = createInstance;
        }

        protected override MethodInfo Route(IDictionary<string, object> environment, 
            IReadOnlyCollection<MethodInfo> availableMethods)
        {
            var command = environment.Get<object>("command");

            return command == null
                ? null
                : availableMethods.FirstOrDefault(x =>
                    x.GetParameters().Any() && x.GetParameters().First().ParameterType == command.GetType());
        }

        protected override object CreateInstance(Type type, IDictionary<string, object> environment)
        {
            return _createInstance(type, environment);
        }

        protected override KeyValuePair<string, string> GetRouteFor(MethodInfo methodInfo)
        {
            var commandType = methodInfo.GetParameters().First().ParameterType;

            return new KeyValuePair<string, string>(commandType.ToString(),
                $"{methodInfo.DeclaringType.Namespace}.{methodInfo.DeclaringType.Name}.{methodInfo.Name}()");
        }

        public static RouteCommandToMethod New(Func<MethodInfo, bool> filter, 
            IReadOnlyCollection<Assembly> applicationAssemblies, 
            Func<Type, IDictionary<string, object>, object> createInstance)
        {
            var methods = applicationAssemblies
                .SelectMany(x => x.GetTypes())
                .SelectMany(x => x.GetMethods(BindingFlags.Instance | BindingFlags.Public))
                .Where(filter)
                .ToList();

            return new RouteCommandToMethod(new ReadOnlyCollection<MethodInfo>(methods), createInstance);
        }
    }
}