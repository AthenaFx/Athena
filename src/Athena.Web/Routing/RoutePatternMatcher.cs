namespace Athena.Web.Routing
{
    public interface RoutePatternMatcher
    {
        RouteMatchResult Match(string requestedPath, string routePath);
    }
}