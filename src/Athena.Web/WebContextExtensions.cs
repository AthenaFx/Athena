using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena.Web
{
    public static class WebContextExtensions
    {
        public static Task ExecuteWebApplication(this AthenaContext context, IDictionary<string, object> environment)
        {
            return context.Execute("web", environment);
        }
    }
}