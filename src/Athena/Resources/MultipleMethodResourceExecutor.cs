using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Athena.Binding;
using Athena.Routing;

namespace Athena.Resources
{
    public class MultipleMethodResourceExecutor : MethodResourceExecutor
    {
        public MultipleMethodResourceExecutor(IReadOnlyCollection<EnvironmentDataBinder> environmentDataBinders) 
            : base(environmentDataBinders)
        {
        }

        public override async Task<ResourceExecutionResult> Execute(RouterResult resource, 
            IDictionary<string, object> environment)
        {
            var multiMethodsResource = resource as MultipleMethodsResourceRouterResult;

            if(multiMethodsResource == null)
                return new ResourceExecutionResult(false, null);
            
            var results = new List<ResourceExecutionResult>();

            foreach (var routerResult in multiMethodsResource.MethodRouterResults)
                results.Add(await base.Execute(routerResult, environment).ConfigureAwait(false));
            
            return new ResourceExecutionResult(results.Any(x => x.Executed), results.Select(x => x.Result).ToList());
        }
    }
}