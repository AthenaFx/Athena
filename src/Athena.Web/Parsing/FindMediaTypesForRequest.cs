using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Routing;

namespace Athena.Web.Parsing
{
    public interface FindMediaTypesForRequest
    {
        Task<IReadOnlyCollection<string>> FindAvailableFor(IDictionary<string, object> environment);
    }
}