using Athena.Routing;

namespace Athena.Web.Sample.Home
{
    public class InValid
    {
        public InValidGetResult Get()
        {
            return new InValidGetResult();
        }

        public EndpointValidationResult ValidateGet()
        {
            return new EndpointValidationResult(false);
        }
    }

    public class InValidGetResult
    {

    }
}