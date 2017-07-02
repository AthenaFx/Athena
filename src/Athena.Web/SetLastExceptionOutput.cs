using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Resources;

namespace Athena.Web
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    
    public class SetLastExceptionOutput
    {
        private readonly AppFunc _next;

        public SetLastExceptionOutput(AppFunc next)
        {
            _next = next;
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            var exception = environment.Get<Exception>("exception");
            
            environment.SetResourceResult(new ExceptionResult(exception));

            return _next(environment);
        }
    }
}