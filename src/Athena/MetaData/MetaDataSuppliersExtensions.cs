using System.Collections.Generic;

namespace Athena.MetaData
{
    public static class MetaDataSuppliersExtensions
    {
        public static IReadOnlyDictionary<string, string> GetDiagnosticsData(
            this IEnumerable<MetaDataSupplier> suppliers)
        {
            var row = 1;
            
            var result = new Dictionary<string, string>();

            foreach (var supplier in suppliers)
            {
                result[row.ToString()] = supplier.ToString();

                row++;
            }

            return result;
        }

    }
}