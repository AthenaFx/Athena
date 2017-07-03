using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Athena.Logging;

namespace Athena.Transactions
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    
    public class HandleTransactions
    {
        private readonly AppFunc _next;
        private readonly IReadOnlyCollection<Transaction> _transactionManagers;

        public HandleTransactions(AppFunc next, IReadOnlyCollection<Transaction> transactionManagers)
        {
            _next = next;
            _transactionManagers = transactionManagers;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            try
            {
                Logger.Write(LogLevel.Debug,
                    $"Starting {_transactionManagers.Count} transactions ({string.Join(", ", _transactionManagers.Select(x => x.GetType().Name))}) for request {environment.GetRequestId()} ({environment.GetCurrentApplication()})");
                
                foreach (var transactionManager in _transactionManagers)
                    await transactionManager.Begin(environment).ConfigureAwait(false);
                
                await _next(environment).ConfigureAwait(false);

                Logger.Write(LogLevel.Debug,
                    $"Commiting {_transactionManagers.Count} transactions ({string.Join(", ", _transactionManagers.Select(x => x.GetType().Name))}) for request {environment.GetRequestId()} ({environment.GetCurrentApplication()})");
                
                foreach (var transactionManager in _transactionManagers)
                    await transactionManager.Commit(environment).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logger.Write(LogLevel.Debug,
                    $"Rolling back {_transactionManagers.Count} transactions ({string.Join(", ", _transactionManagers.Select(x => x.GetType().Name))}) for request {environment.GetRequestId()} ({environment.GetCurrentApplication()})");
                
                foreach (var transactionManager in _transactionManagers)
                    await transactionManager.Rollback(environment, e).ConfigureAwait(false);
                
                throw;
            }
        }
    }
}