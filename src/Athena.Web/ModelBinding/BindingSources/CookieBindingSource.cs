using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Athena.Web.ModelBinding.BindingSources
{
    public class CookieBindingSource : BindingSource
    {
        public Task<IReadOnlyDictionary<string, object>> GetValues(IDictionary<string, object> envinronment)
        {
            return Task.FromResult((IReadOnlyDictionary<string, object>)envinronment.GetRequest().Cookies.ToDictionary(x => x.Key.ToLower(), x => (object)x.Value));
        }
    }
}