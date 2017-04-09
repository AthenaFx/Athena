using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Logging;

namespace Athena
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class HandleExceptions
    {
        private readonly AppFunc _next;
        private readonly Func<Exception, IDictionary<string, object>, Task> _onError;

        public HandleExceptions(AppFunc next, Func<Exception, IDictionary<string, object>, Task> onError = null)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));

            _onError = onError ?? ((x, y) => Task.CompletedTask);
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            try
            {
                await _next(environment).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                environment["exception"] = ex;

                await _onError(ex, environment);

                Logger.Write(LogLevel.Error, $"Exception while executing application: {environment.GetCurrentApplication()}", ex);
            }
        }
    }}