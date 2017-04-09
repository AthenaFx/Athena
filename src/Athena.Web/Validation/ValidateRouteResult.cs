using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Routing;

namespace Athena.Web.Validation
{
    public interface ValidateRouteResult
    {
        Task<ValidationResult> Validate(RouterResult result, IDictionary<string, object> environment);
    }
}