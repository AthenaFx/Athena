using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
                foreach (var transactionManager in _transactionManagers)
                    await transactionManager.Begin(environment).ConfigureAwait(false);
                
                await _next(environment).ConfigureAwait(false);

                foreach (var transactionManager in _transactionManagers)
                    await transactionManager.Commit(environment).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                foreach (var transactionManager in _transactionManagers)
                    await transactionManager.Rollback(environment, e).ConfigureAwait(false);
                
                throw;
            }
        }
    }
}