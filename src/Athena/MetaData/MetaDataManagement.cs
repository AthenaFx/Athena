using System.Collections.Generic;

namespace Athena.MetaData
{
    public static class MetaDataManagement
    {
        public const string MetaDataKey = "application-meta-data";
        
        public static IReadOnlyDictionary<string, object> GetMetaData(this IDictionary<string, object> environment)
        {
            return environment.Get<IReadOnlyDictionary<string, object>>(MetaDataKey,
                new Dictionary<string, object>());
        }
    }
}