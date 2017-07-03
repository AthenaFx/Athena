using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Athena.Logging;

namespace Athena.MetaData
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    
    public class SupplyMetaData
    {
        private readonly AppFunc _next;
        private readonly IReadOnlyCollection<MetaDataSupplier> _metaDataSuppliers;

        public SupplyMetaData(AppFunc next, IReadOnlyCollection<MetaDataSupplier> metaDataSuppliers)
        {
            _next = next;
            _metaDataSuppliers = metaDataSuppliers;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            Logger.Write(LogLevel.Debug,
                $"Supplying meta data for request {environment.GetRequestId()} ({environment.GetCurrentApplication()})");
            
            using (new MetaDataDisposable(_metaDataSuppliers, environment))
            {
                await _next(environment).ConfigureAwait(false);
            }
        }
        
        private class MetaDataDisposable : IDisposable
        {
            private readonly IReadOnlyDictionary<string, object> _previousMetaData;
            private readonly IDictionary<string, object> _environment;

            public MetaDataDisposable(IEnumerable<MetaDataSupplier> suppliers, 
                IDictionary<string, object> environment)
            {
                var previousMetaData = environment.GetMetaData();
                
                _previousMetaData = previousMetaData;
                _environment = environment;

                var newMetaData = (previousMetaData ?? new Dictionary<string, object>())
                    .ToDictionary(x => x.Key, x => x.Value);

                foreach (var supplier in suppliers)
                {
                    foreach (var item in supplier.GetFrom(environment))
                        newMetaData[item.Key] = item.Value;
                }

                environment[MetaDataManagement.MetaDataKey] = new ReadOnlyDictionary<string, object>(newMetaData);
            }

            public void Dispose()
            {
                _environment[MetaDataManagement.MetaDataKey] = _previousMetaData;
            }
        }

    }
}