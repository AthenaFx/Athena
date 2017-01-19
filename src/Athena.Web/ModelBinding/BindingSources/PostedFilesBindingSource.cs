using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Athena.Web.ModelBinding.BindingSources
{
    public class PostedFilesBindingSource : BindingSource
    {
        public async Task<IReadOnlyDictionary<string, object>> GetValues(IDictionary<string, object> envinronment)
        {
            var files = await envinronment.GetRequest().ReadFiles().ConfigureAwait(false);

            return files.ToDictionary(x => x.Name.ToLower(), x => (object)new PostedFile(x.Name, x.ContentType, x.Value));
        }
    }
}