using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena.EventStore.ProcessManagers
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    
    public class ExecuteProcessManager
    {
        private readonly AppFunc _next;

        public ExecuteProcessManager(AppFunc next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }
        
        public async Task Invoke(IDictionary<string, object> environment)
        {
            var context = environment.Get<ProcessManagerExecutionContext>("context");

            if (context != null)
            {
                await context
                    .ProcessManager
                    .Handle(context.Event, environment,
                        environment.GetAthenaContext(), context.Serializer, context.Connection)
                    .ConfigureAwait(false);
            }

            await _next(environment).ConfigureAwait(false);
        }
    }
}