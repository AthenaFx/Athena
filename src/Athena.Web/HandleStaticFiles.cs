using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Athena.Routing;

namespace Athena.Web
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class HandleStaticFiles
    {
        private readonly AppFunc _next;
        private readonly IReadOnlyCollection<StaticFileReader> _fileReaders;

        public HandleStaticFiles(AppFunc next, IReadOnlyCollection<StaticFileReader> fileReaders)
        {
            _next = next;
            _fileReaders = fileReaders;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var file = environment.GetRequest().Uri.LocalPath;
            var response = environment.GetResponse();
            var acceptedMediaTypes = environment.GetRequest().Headers.GetAcceptedMediaTypes().ToList();
            var executeNext = true;

            foreach (var fileReader in _fileReaders)
            {
                var readResult = await fileReader.TryRead(environment, file).ConfigureAwait(false);

                if (!readResult.Exists)
                    continue;

                if (!acceptedMediaTypes.Any(x => x.Matches(readResult.ContentType)))
                {
                    executeNext = false;
                    response.StatusCode = 406;

                    continue;
                }

                environment["endpointresults"] = new EndpointExecutionResult(true,
                    new CachedFileResult(readResult.CacheData));

                await response.Write(readResult.Read()).ConfigureAwait(false);

                response.Headers.ContentType = readResult.ContentType;

                response.StatusCode = 200;

                return;
            }

            if (executeNext)
                await _next(environment);
        }
    }
}