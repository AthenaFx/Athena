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

        public HandleExceptions(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            _next = next;
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

                Logger.Write(LogLevel.Error, $"Exception while executing application: {environment.GetCurrentApplication()}", ex);
            }
        }
    }}