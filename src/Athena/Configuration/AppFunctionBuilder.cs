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
        private readonly IDictionary<string, Tuple<Func<AppFunc, AppFunc>, Func<IReadOnlyDictionary<string, string>>>>
            _appFunctionFactories
                = new ConcurrentDictionary<string,
                    Tuple<Func<AppFunc, AppFunc>, Func<IReadOnlyDictionary<string, string>>>>();

        private readonly LinkedList<string> _chain = new LinkedList<string>();
        private readonly ICollection<Func<AppFunc, string, AppFunc>> _wrappers 
            = new List<Func<AppFunc, string, AppFunc>>();

        public AppFunctionBuilder(AthenaBootstrapper bootstrapper)
        {
            Bootstrapper = bootstrapper;
        }

        public AthenaBootstrapper Bootstrapper { get; }

        public AppFunctionBuilder Replace(string item, string name, Func<AppFunc, AppFunc> builder, 
            Func<IReadOnlyDictionary<string, string>> getDiagnosticsData = null)
        {
            _appFunctionFactories[name] =
                new Tuple<Func<AppFunc, AppFunc>, Func<IReadOnlyDictionary<string, string>>>(builder,
                    getDiagnosticsData ?? (() => new Dictionary<string, string>()));

            _chain.Find(item).Value = name;

            return this;
        }
        
        public AppFunctionBuilder Replace(string item, Func<AppFunc, AppFunc> builder,
            Func<IReadOnlyDictionary<string, string>> getDiagnosticsData = null)
        {
            return Replace(item, item, builder, getDiagnosticsData);
        }

        public AppFunctionBuilder Before(string item, string name, Func<AppFunc, AppFunc> builder,
            Func<IReadOnlyDictionary<string, string>> getDiagnosticsData = null)
        {
            _appFunctionFactories[name] =
                new Tuple<Func<AppFunc, AppFunc>, Func<IReadOnlyDictionary<string, string>>>(builder,
                    getDiagnosticsData ?? (() => new Dictionary<string, string>()));

            _chain.AddBefore(_chain.Find(item), name);

            return this;
        }

        public AppFunctionBuilder After(string item, string name, Func<AppFunc, AppFunc> builder,
            Func<IReadOnlyDictionary<string, string>> getDiagnosticsData = null)
        {
            _appFunctionFactories[name] =
                new Tuple<Func<AppFunc, AppFunc>, Func<IReadOnlyDictionary<string, string>>>(builder,
                    getDiagnosticsData ?? (() => new Dictionary<string, string>()));

            _chain.AddAfter(_chain.Find(item), name);

            return this;
        }

        public AppFunctionBuilder First(string item, Func<AppFunc, AppFunc> builder,
            Func<IReadOnlyDictionary<string, string>> getDiagnosticsData = null)
        {
            _appFunctionFactories[item] =
                new Tuple<Func<AppFunc, AppFunc>, Func<IReadOnlyDictionary<string, string>>>(builder,
                    getDiagnosticsData ?? (() => new Dictionary<string, string>()));

            _chain.AddFirst(item);

            return this;
        }

        public AppFunctionBuilder Last(string item, Func<AppFunc, AppFunc> builder,
            Func<IReadOnlyDictionary<string, string>> getDiagnosticsData = null)
        {
            _appFunctionFactories[item] =
                new Tuple<Func<AppFunc, AppFunc>, Func<IReadOnlyDictionary<string, string>>>(builder,
                    getDiagnosticsData ?? (() => new Dictionary<string, string>()));

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

        public Tuple<AppFunc, IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>> Compile()
        {
            var result = new List<Func<AppFunc, AppFunc>>();
            var diagnosticsData = new Dictionary<string, IReadOnlyDictionary<string, string>>();

            foreach (var item in _chain)
            {
                var currentItemName = item;
                
                result.AddRange(_wrappers
                    .Select<Func<AppFunc, string, AppFunc>, Func<AppFunc, AppFunc>>(x => (y => x(y, currentItemName))));

                var row = _appFunctionFactories[item];
                
                result.Add(row.Item1);

                diagnosticsData[item] = row.Item2();
            }

            result.Reverse();
            diagnosticsData = diagnosticsData
                .Reverse()
                .ToDictionary(x => x.Key, x => x.Value);
            
            Task LastFunc(IDictionary<string, object> x) => Task.CompletedTask;

            var application = result.Aggregate((AppFunc) LastFunc, (current, item) => item(current));
            
            return new Tuple<AppFunc, IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>>
                (application, diagnosticsData);
        }
    }
}