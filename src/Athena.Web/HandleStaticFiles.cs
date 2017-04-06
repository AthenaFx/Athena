using System;
using System.Collections.Generic;
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

            foreach (var fileReader in _fileReaders)
            {
                var readResult = await fileReader.TryRead(environment, file).ConfigureAwait(false);

                if(!readResult.Exists)
                    continue;

                environment["endpointresults"] = new EndpointExecutionResult(true,
                    new CachedFileResult(readResult.CacheData));

                await environment.GetResponse().Write(readResult.Read()).ConfigureAwait(false);

                return;
            }

            await _next(environment);
        }
    }
}