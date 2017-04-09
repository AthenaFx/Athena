using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Routing;

namespace Athena.Resources
{
    public interface ResourceExecutor
    {

    }

    public interface ResourceExecutor<in TResource> : ResourceExecutor where TResource : RouterResult
    {
        Task<object> Execute(TResource resource, IDictionary<string, object> environment);
    }
}