using System;
using System.Threading.Tasks;
using Athena.Logging;

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
            Logger.Write(LogLevel.Debug, $"Starting conditional process {_inner}");
            
            _started = true;

            if (_shouldRun && !_isRunning)
            {
                await _inner.Start(context).ConfigureAwait(false);

                _isRunning = true;
            }
        }

        public async Task Stop(AthenaContext context)
        {
            Logger.Write(LogLevel.Debug, $"Stopping conditional process {_inner}");
            
            _started = false;

            if (_isRunning)
            {
                await _inner.Stop(context).ConfigureAwait(false);

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
                await _inner.Stop(context).ConfigureAwait(false);

                _isRunning = false;
            }
        }
    }
}