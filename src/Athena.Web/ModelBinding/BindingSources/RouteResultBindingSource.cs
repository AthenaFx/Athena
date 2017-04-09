using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Routing;

namespace Athena.Web.ModelBinding.BindingSources
{
    public class RouteResultBindingSource : BindingSource
    {
        public Task<IReadOnlyDictionary<string, object>> GetValues(IDictionary<string, object> envinronment)
        {
            var routeResult = envinronment.GetRouteResult();

            return Task.FromResult(routeResult?.GetParameters() ?? new Dictionary<string, object>());
        }
    }
}