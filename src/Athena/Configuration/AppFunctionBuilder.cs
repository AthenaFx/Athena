using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Athena.Configuration
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class AppFunctionBuilder
    {
        private readonly IDictionary<string, Func<AppFunc, AppFunc>> _appFunctionFactories
            = new ConcurrentDictionary<string, Func<AppFunc, AppFunc>>();

        private readonly LinkedList<string> _chain = new LinkedList<string>();
        private readonly ICollection<Func<AppFunc, string, AppFunc>> _wrappers 
            = new List<Func<AppFunc, string, AppFunc>>();

        public AppFunctionBuilder(AthenaBootstrapper bootstrapper)
        {
            Bootstrapper = bootstrapper;
        }

        public AthenaBootstrapper Bootstrapper { get; }

        public AppFunctionBuilder Replace(string item, string name, Func<AppFunc, AppFunc> builder)
        {
            _appFunctionFactories[name] = builder;

            _chain.Find(item).Value = name;

            return this;
        }
        
        public AppFunctionBuilder Replace(string item, Func<AppFunc, AppFunc> builder)
        {
            return Replace(item, item, builder);
        }

        public AppFunctionBuilder Before(string item, string name, Func<AppFunc, AppFunc> builder)
        {
            _appFunctionFactories[name] = builder;

            _chain.AddBefore(_chain.Find(item), name);

            return this;
        }

        public AppFunctionBuilder After(string item, string name, Func<AppFunc, AppFunc> builder)
        {
            _appFunctionFactories[name] = builder;

            _chain.AddAfter(_chain.Find(item), name);

            return this;
        }

        public AppFunctionBuilder First(string item, Func<AppFunc, AppFunc> builder)
        {
            _appFunctionFactories[item] = builder;

            _chain.AddFirst(item);

            return this;
        }

        public AppFunctionBuilder Last(string item, Func<AppFunc, AppFunc> builder)
        {
            _appFunctionFactories[item] = builder;

            _chain.AddLast(item);

            return this;
        }

        public AppFunctionBuilder Remove(string item)
        {
            _appFunctionFactories.Remove(item);
            _chain.Remove(item);

            return this;
        }

        public AppFunctionBuilder WrapAllWith(Func<AppFunc, string, AppFunc> builder)
        {
            _wrappers.Add(builder);

            return this;
        }

        public AppFunctionBuilder Reset()
        {
            _appFunctionFactories.Clear();
            _chain.Clear();

            return this;
        }

        public AppFunc Compile()
        {
            var result = new List<Func<AppFunc, AppFunc>>();

            foreach (var item in _chain)
            {
                var currentItemName = item;
                
                result.AddRange(_wrappers
                    .Select<Func<AppFunc, string, AppFunc>, Func<AppFunc, AppFunc>>(x => (y => x(y, currentItemName))));
                
                result.Add(_appFunctionFactories[item]);
            }

            result.Reverse();

            Task LastFunc(IDictionary<string, object> x) => Task.CompletedTask;

            return result.Aggregate((AppFunc) LastFunc, (current, item) => item(current));
        }
    }
}