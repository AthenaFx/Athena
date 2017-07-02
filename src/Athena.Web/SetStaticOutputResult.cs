using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Resources;

namespace Athena.Web
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    
    public class SetStaticOutputResult
    {
        private readonly AppFunc _next;
        private readonly Func<IDictionary<string, object>, object> _getOutput;

        public SetStaticOutputResult(AppFunc next, Func<IDictionary<string, object>, object> getOutput)
        {
            _next = next;
            _getOutput = getOutput;
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            environment.SetResourceResult(_getOutput(environment));

            return _next(environment);
        }
    }
}