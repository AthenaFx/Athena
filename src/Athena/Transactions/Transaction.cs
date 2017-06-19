using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena.Transactions
{
    public interface Transaction
    {
        Task Begin(IDictionary<string, object> environment);
        Task Commit(IDictionary<string, object> environment);
        Task Rollback(IDictionary<string, object> environment, Exception exception = null);
    }
}