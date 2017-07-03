using System.Collections.Generic;

namespace Athena.Transactions
{
    public static class TransactionsExtensions
    {
        public static IReadOnlyDictionary<string, string> GetDiagnosticsData(this IEnumerable<Transaction> transactions)
        {
            var row = 1;
            
            var result = new Dictionary<string, string>();

            foreach (var transaction in transactions)
            {
                result[row.ToString()] = transaction.ToString();

                row++;
            }

            return result;
        }
    }
}