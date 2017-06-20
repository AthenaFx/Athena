using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class Retry
    {
        private readonly AppFunc _next;
        private readonly int _retryTimes;
        private readonly TimeSpan _retryInterval;
        private readonly string _description;

        public Retry(AppFunc next, int retryTimes, TimeSpan retryInterval, string description)
        {
            if (retryTimes < 1)
                throw new ArgumentException("You have to try atleast once", nameof(retryTimes));

            _next = next ?? throw new ArgumentNullException(nameof(next));
            _retryTimes = retryTimes;
            _retryInterval = retryInterval;
            _description = description;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var tries = 0;
            Exception lastException = null;

            while (tries < _retryTimes)
            {
                try
                {
                    await _next(environment).ConfigureAwait(false);

                    return;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }

                await Task.Delay(_retryInterval).ConfigureAwait(false);

                tries++;
            }

            if (lastException != null)
                throw new RetryException(_retryTimes, _description, lastException);
        }
    }
}