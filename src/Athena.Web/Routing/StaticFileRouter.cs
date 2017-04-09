using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Routing;

namespace Athena.Web.Routing
{
    public class StaticFileRouter : EnvironmentRouter
    {
        private readonly IReadOnlyCollection<StaticFileReader> _fileReaders;

        public StaticFileRouter(IReadOnlyCollection<StaticFileReader> fileReaders)
        {
            _fileReaders = fileReaders;
        }

        public async Task<RouterResult> Route(IDictionary<string, object> environment)
        {
            var file = environment.GetRequest().Uri.LocalPath;

            foreach (var fileReader in _fileReaders)
            {
                var readResult = await fileReader.TryRead(environment, file).ConfigureAwait(false);

                if (readResult.Exists)
                    return new StaticFileRouterResult(readResult.ContentType, readResult.Read, readResult.CacheData);
            }

            return null;
        }
    }
}