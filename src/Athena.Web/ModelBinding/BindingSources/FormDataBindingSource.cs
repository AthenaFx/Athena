using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Athena.Web.ModelBinding.BindingSources
{
    public class FormDataBindingSource : BindingSource
    {
        public async Task<IReadOnlyDictionary<string, object>> GetValues(IDictionary<string, object> envinronment)
        {
            var form = await envinronment.GetRequest().ReadForm().ConfigureAwait(false);
            return form.ToDictionary(x => x.Key.ToLower(), x => (object)form.Get(x.Key));
        }
    }
}