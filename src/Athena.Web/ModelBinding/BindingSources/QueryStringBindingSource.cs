using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Athena.Web.ModelBinding.BindingSources
{
    public class QueryStringBindingSource : BindingSource
    {
        public Task<IReadOnlyDictionary<string, object>> GetValues(IDictionary<string, object> envinronment)
        {
            return Task.FromResult((IReadOnlyDictionary<string, object>)envinronment.GetRequest().Query.ToDictionary(x => x.Key.ToLower(), x => (object)x.Value));
        }
    }
}