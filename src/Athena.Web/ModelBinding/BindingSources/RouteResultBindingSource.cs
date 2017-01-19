using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Routing;

namespace Athena.Web.ModelBinding.BindingSources
{
    public class RouteResultBindingSource : BindingSource
    {
        public Task<IReadOnlyDictionary<string, object>> GetValues(IDictionary<string, object> envinronment)
        {
            return Task.FromResult(envinronment.Get("route-result",
                new RouterResult(false, null, new Dictionary<string, object>())).Parameters);
        }
    }
}