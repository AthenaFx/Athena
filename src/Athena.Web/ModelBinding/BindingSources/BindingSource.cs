using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena.Web.ModelBinding.BindingSources
{
    public interface BindingSource
    {
        Task<IReadOnlyDictionary<string, object>> GetValues(IDictionary<string, object> envinronment);
    }
}