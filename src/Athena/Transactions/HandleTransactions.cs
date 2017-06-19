using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena.Transactions
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    
    public class HandleTransactions
    {
        private readonly AppFunc _next;

        public HandleTransactions(AppFunc next)
        {
            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var transaction = await TransactionManager.BeginTransaction(environment).ConfigureAwait(false);
            
            try
            {
                await _next(environment).ConfigureAwait(false);

                await transaction.Commit(environment).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                await transaction.Rollback(environment, e).ConfigureAwait(false);
                
                throw;
            }
        }
    }
}