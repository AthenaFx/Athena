using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Athena.MetaData
{
    public static class MetaDataManagement
    {
        private static readonly
            ICollection<Tuple<Func<IDictionary<string, object>, bool>,
                Func<IDictionary<string, object>, MetaDataSupplier>>> Suppliers =
                new List<Tuple<Func<IDictionary<string, object>, bool>,
                    Func<IDictionary<string, object>, MetaDataSupplier>>>();

        public static AthenaBootstrapper HandleMetaDataUsing(this AthenaBootstrapper bootstrapper, 
            Func<IDictionary<string, object>, MetaDataSupplier> getSupplier,
            Func<IDictionary<string, object>, bool> filter)
        {
            Suppliers.Add(
                new Tuple<Func<IDictionary<string, object>, bool>, Func<IDictionary<string, object>, MetaDataSupplier>>(
                    filter, getSupplier));

            return bootstrapper;
        }

        public static AthenaBootstrapper HandleMetaDataForApplicationUsing(this AthenaBootstrapper bootstrapper,
            Func<IDictionary<string, object>, MetaDataSupplier> getSupplier,
            string application)
        {
            HandleMetaDataUsing(bootstrapper, getSupplier, x => x.GetCurrentApplication() == application);

            return bootstrapper;
        }

        internal static IDisposable GatherMetaData(IDictionary<string, object> environment)
        {
            var suppliers = Suppliers
                .Where(x => x.Item1(environment))
                .Select(x => x.Item2(environment))
                .ToList();

            return new MetaDataDisposable(suppliers, environment);
        }
        
        private class MetaDataDisposable : IDisposable
        {
            private readonly IReadOnlyDictionary<string, object> _previousMetaData;
            private readonly IDictionary<string, object> _environment;

            public MetaDataDisposable(IEnumerable<MetaDataSupplier> suppliers, 
                IDictionary<string, object> environment)
            {
                var previousMetaData =
                    environment.Get<IReadOnlyDictionary<string, object>>("application-meta-data",
                        new Dictionary<string, object>());
                
                _previousMetaData = previousMetaData;
                _environment = environment;

                var newMetaData = (previousMetaData ?? new Dictionary<string, object>())
                    .ToDictionary(x => x.Key, x => x.Value);

                foreach (var supplier in suppliers)
                {
                    foreach (var item in supplier.GetFrom(environment))
                        newMetaData[item.Key] = item.Value;
                }

                environment["application-meta-data"] = new ReadOnlyDictionary<string, object>(newMetaData);
            }

            public void Dispose()
            {
                _environment["application-meta-data"] = _previousMetaData;
            }
        }
    }
}