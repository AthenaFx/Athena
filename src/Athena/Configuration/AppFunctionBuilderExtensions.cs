using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena.Configuration
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    
    public static class AppFunctionBuilderExtensions
    {
        public static AppFunctionBuilder ContinueWith(this AppFunctionBuilder builder, string name, 
            Func<AppFunc, AppFunc> conf, Func<IReadOnlyDictionary<string, string>> getDiagnosticsData = null)
        {
            return builder.Last(name, conf, getDiagnosticsData);
        }
    }
}