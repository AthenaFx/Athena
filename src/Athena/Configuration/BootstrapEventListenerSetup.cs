using System;
using System.Threading.Tasks;

namespace Athena.Configuration
{
    public class BootstrapEventListenerSetup<TEvent> where TEvent : SetupEvent
    {
        private readonly Action<Func<TEvent, AthenaSetupContext, Task>> _addListener;
        private readonly AthenaBootstrapper _bootstrapper;

        public BootstrapEventListenerSetup(Action<Func<TEvent, AthenaSetupContext, Task>> addListener, 
            AthenaBootstrapper bootstrapper)
        {
            _addListener = addListener;
            _bootstrapper = bootstrapper;
        }

        public AthenaBootstrapper Do(Func<TEvent, AthenaSetupContext, Task> execute)
        {
            _addListener(execute);

            return _bootstrapper;
        }

        public AthenaBootstrapper Do(Action<TEvent, AthenaSetupContext> execute)
        {
            return Do((evnt, context) =>
            {
                execute(evnt, context);

                return Task.CompletedTask;
            });
        }
    }
}