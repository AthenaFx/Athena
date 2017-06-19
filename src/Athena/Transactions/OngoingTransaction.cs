using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena.Transactions
{
    public class OngoingTransaction
    {
        public OngoingTransaction(Func<IDictionary<string, object>, Task> commit, 
            Func<IDictionary<string, object>, Exception, Task> rollback)
        {
            Commit = commit;
            Rollback = rollback;
        }

        public Func<IDictionary<string, object>, Task> Commit { get; }
        public Func<IDictionary<string, object>, Exception, Task> Rollback { get; }
    }
}