using System;
using System.Threading.Tasks;
using Athena.Configuration;

namespace Athena.Processes
{
    public class ConditionedProcessWrapper : LongRunningProcess
    {
        private readonly LongRunningProcess _inner;
        private bool _isRunning;
        private bool _shouldRun;
        private bool _started;

        public ConditionedProcessWrapper(LongRunningProcess inner, 
            Func<Func<bool, AthenaContext, Task>, Task> subscribeToChanges)
        {
            _inner = inner;
            subscribeToChanges(ChangeRunning);
        }

        public async Task Start(AthenaContext context)
        {
            _started = true;

            if (_shouldRun && !_isRunning)
            {
                await _inner.Start(context).ConfigureAwait(false);

                _isRunning = true;
            }
        }

        public async Task Stop()
        {
            _started = false;

            if (_isRunning)
            {
                await _inner.Stop().ConfigureAwait(false);

                _isRunning = false;
            }
        }

        private async Task ChangeRunning(bool shouldRun, AthenaContext context)
        {
            //TODO:Refactor and make sure we lock correctly
            _shouldRun = shouldRun;

            if (!_started)
                return;

            if (_shouldRun && !_isRunning)
            {
                await _inner.Start(context).ConfigureAwait(false);

                _isRunning = true;
            }
            else if(!_shouldRun && _isRunning)
            {
                await _inner.Stop().ConfigureAwait(false);

                _isRunning = false;
            }
        }
    }
}