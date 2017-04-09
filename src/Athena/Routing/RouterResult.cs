using System.Collections.Generic;

namespace Athena.Routing
{
    public interface RouterResult
    {
        IReadOnlyDictionary<string, object> GetParameters();
    }
}