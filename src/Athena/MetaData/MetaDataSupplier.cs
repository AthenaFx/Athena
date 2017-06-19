using System.Collections.Generic;

namespace Athena.MetaData
{
    public interface MetaDataSupplier
    {
        IReadOnlyDictionary<string, object> GetFrom(IDictionary<string, object> environment);
    }
}