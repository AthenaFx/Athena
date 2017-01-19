using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Athena
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public static class AppFunctions
    {
        public static Builder StartWith<T>(params object[] args)
        {
            var builder = new Builder();

            return builder.Then<T>(args);
        }

        public class Builder
        {
            private readonly ICollection<MiddlewareWithArgs> _middlewares = new Collection<MiddlewareWithArgs>();

            public Builder Then<T>(params object[] args)
            {
                _middlewares.Add(new MiddlewareWithArgs(typeof(T), args));

                return this;
            }

            public AppFunc Build()
            {
                var list = new List<MiddlewareWithArgs>(_middlewares);

                list.Reverse();

                AppFunc lastFunc = x => Task.CompletedTask;

                foreach (var item in list)
                {
                    var args = new List<object>
                    {
                        lastFunc
                    };

                    args.AddRange(item.Args);

                    var constructor = item.Type.GetConstructor(args.Select(x => x.GetType()).ToArray());

                    var instance = constructor.Invoke(args.ToArray());

                    var method = instance.GetType().GetMethod("Invoke", new[] { typeof(IDictionary<string, object>) });

                    var parameter = Expression.Parameter(typeof(IDictionary<string, object>));

                    lastFunc = Expression.Lambda<AppFunc>(Expression.Call(Expression.Constant(instance), method, parameter), parameter).Compile();
                }

                return lastFunc;
            }

            private class MiddlewareWithArgs
            {
                public MiddlewareWithArgs(Type type, object[] args)
                {
                    Type = type;
                    Args = args;
                }

                public Type Type { get; }
                public object[] Args { get; }
            }
        }
    }
}