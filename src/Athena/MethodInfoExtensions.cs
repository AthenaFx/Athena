using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Athena
{
    public static class MethodInfoExtensions
    {
        private static readonly ConcurrentDictionary<MethodInfo, Func<object, object[], object>> Methods =
            new ConcurrentDictionary<MethodInfo, Func<object, object[], object>>();
        
        public static async Task<T> CompileAndExecute<T>(this MethodInfo methodInfo, object instance, 
            Func<Type, Task<object>> getInput)
        {
            var parameters = new List<object>();

            foreach (var parameter in methodInfo.GetParameters())
                parameters.Add(await getInput(parameter.ParameterType).ConfigureAwait(false));
            
            return (T)Methods.GetOrAdd(methodInfo, x =>
            {
                var methodParams = x.GetParameters();
                var arrayParameter = Expression.Parameter(typeof(object[]), "array");

                var arguments =
                    methodParams.Select((p, i) => Expression.Convert(
                            Expression.ArrayAccess(arrayParameter, Expression.Constant(i)), p.ParameterType))
                        .Cast<Expression>()
                        .ToList();

                var instanceParameter = Expression.Parameter(typeof(object), "controller");

                var instanceExp = Expression.Convert(instanceParameter, x.DeclaringType);
                var callExpression = Expression.Call(instanceExp, x, arguments);

                var bodyExpression = Expression.Convert(callExpression, typeof(object));

                return Expression.Lambda<Func<object, object[], object>>(
                        bodyExpression, instanceParameter, arrayParameter)
                    .Compile();
            })(instance, parameters.ToArray());
        }
    }
}