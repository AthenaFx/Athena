using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Athena.Transactions
{
    public static class TransactionManager
    {
        private static readonly ICollection<Tuple<Func<IDictionary<string, object>, bool>, Func<IDictionary<string, object>, Transaction>>> 
            TransactionManagers = new List<Tuple<Func<IDictionary<string, object>, bool>, Func<IDictionary<string, object>, Transaction>>>();

        public static AthenaBootstrapper HandleTransactionsUsing(this AthenaBootstrapper bootstrapper, 
            Func<IDictionary<string, object>, Transaction> getTransaction,
            Func<IDictionary<string, object>, bool> filter)
        {
            TransactionManagers.Add(new Tuple<Func<IDictionary<string, object>, bool>, 
                Func<IDictionary<string, object>, Transaction>>(filter, getTransaction));

            return bootstrapper;
        }

        public static AthenaBootstrapper HandleTransactionsForApplicationUsing(this AthenaBootstrapper bootstrapper,
            Func<IDictionary<string, object>, Transaction> getTransaction, string application)
        {
            HandleTransactionsUsing(bootstrapper, getTransaction, x => x.GetCurrentApplication() == application);

            return bootstrapper;
        }
        
        internal static async Task<OngoingTransaction> BeginTransaction(IDictionary<string, object> environment)
        {
            var transactions = TransactionManagers
                .Where(x => x.Item1(environment))
                .Select(x => x.Item2(environment))
                .ToList();

            foreach (var transaction in transactions)
                await transaction.Begin(environment).ConfigureAwait(false);

            async Task Commit(IDictionary<string, object> e)
            {
                foreach (var transaction in transactions)
                    await transaction.Commit(e);
            }

            async Task Rollback(IDictionary<string, object> env, Exception exception)
            {
                foreach (var transaction in transactions)
                    await transaction.Rollback(env, exception);
            }

            return new OngoingTransaction(Commit, Rollback);
        }
    }
}