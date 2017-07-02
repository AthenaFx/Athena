using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena.Web.Parsing
{
    public class StaticMediaTypeFinder : FindMediaTypesForRequest
    {
        private readonly IReadOnlyCollection<string> _availableMediaTypes;

        public StaticMediaTypeFinder(params string[] availableMediaTypes)
        {
            _availableMediaTypes = availableMediaTypes;
        }

        public Task<IReadOnlyCollection<string>> FindAvailableFor(IDictionary<string, object> environment)
        {
            return Task.FromResult(_availableMediaTypes);
        }
    }
}