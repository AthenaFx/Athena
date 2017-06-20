using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena.EventStore.Projections
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ExecuteProjection
    {
        private readonly AppFunc _next;

        public ExecuteProjection(AppFunc next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var context = environment.Get<ProjectionContext>("context");

            if (context != null)
            {
                foreach (var evnt in context.Events)
                {
                    await context.Projection.Apply(evnt, environment).ConfigureAwait(false);

                    context.Handled(evnt);
                }
            }

            await _next(environment).ConfigureAwait(false);
        }
    }
}