using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena.Web
{
    public interface StaticFileReader
    {
        Task<FileReadResult> TryRead(IDictionary<string, object> environment, string file);
    }
}