using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Athena
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public static class AppFunctions
    {
        public static Builder StartWith(Func<AppFunc, AppFunc> functionBuilder)
        {
            var builder = new Builder();

            return builder.Then(functionBuilder);
        }

        public class Builder
        {
            private readonly ICollection<Func<AppFunc, AppFunc>> _middlewares = new Collection<Func<AppFunc, AppFunc>>();

            public Builder Then(Func<AppFunc, AppFunc> functionBuilder)
            {
                _middlewares.Add(functionBuilder);

                return this;
            }

            public AppFunc Build()
            {
                var list = new List<Func<AppFunc, AppFunc>>(_middlewares);

                list.Reverse();

                AppFunc lastFunc = x => Task.CompletedTask;

                return list.Aggregate(lastFunc, (current, item) => item(current));
            }
        }
    }
}