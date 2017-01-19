using System.Collections.Generic;

namespace Athena.Web.Routing
{
    public class Route
    {
        public Route(string pattern, object destination, IReadOnlyCollection<string> availableHttpMethods)
        {
            Pattern = pattern;
            Destination = destination;
            AvailableHttpMethods = availableHttpMethods;
        }

        public string Pattern { get; }
        public object Destination { get; }
        public IReadOnlyCollection<string> AvailableHttpMethods { get; }
    }
}