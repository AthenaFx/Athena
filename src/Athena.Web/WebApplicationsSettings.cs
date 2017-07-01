using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Athena.Configuration;

namespace Athena.Web
{
    public class WebApplicationsSettings
    {
        private readonly ConcurrentDictionary<string, Tuple<int, Func<IDictionary<string, object>, bool>,
            Func<AppFunctionBuilder, AppFunctionBuilder>>> _webApplications =
            new ConcurrentDictionary<string, Tuple<int, Func<IDictionary<string, object>, bool>,
                Func<AppFunctionBuilder, AppFunctionBuilder>>>();

        public WebApplicationsSettings AddApplication(string name, 
            Func<AppFunctionBuilder, AppFunctionBuilder> defaultBuilder, 
            Func<IDictionary<string, object>, bool> filter = null)
        {
            var order = 1;

            if (filter == null)
                order = int.MaxValue;

            filter = filter ?? (x => true);
            
            _webApplications[name] = new Tuple<int, Func<IDictionary<string, object>, bool>, 
                Func<AppFunctionBuilder, AppFunctionBuilder>>(order, filter, defaultBuilder);

            return this;
        }

        public WebApplicationsSettings ConfigureApplication(string name,
            Func<AppFunctionBuilder, AppFunctionBuilder> builder,
            Func<IDictionary<string, object>, bool> filter = null)
        {
            if(!_webApplications.ContainsKey(name))
                throw new InvalidOperationException($"There is no application named {name}");

            var currentItem = _webApplications[name];
            
            var order = 1;

            if (filter == null)
                order = int.MaxValue;

            filter = filter ?? (x => true);
            
            _webApplications[name] = new Tuple<int, Func<IDictionary<string, object>, bool>, 
                Func<AppFunctionBuilder, AppFunctionBuilder>>(order, filter, x => builder(currentItem.Item3(x)));

            return this;
        }

        internal IReadOnlyDictionary<string, Tuple<int, Func<IDictionary<string, object>, bool>,
            Func<AppFunctionBuilder, AppFunctionBuilder>>> GetApplications()
        {
            return _webApplications;
        }
    }
}