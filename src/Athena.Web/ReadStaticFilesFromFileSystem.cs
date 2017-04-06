using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Athena.Web
{
    public class ReadStaticFilesFromFileSystem : StaticFileReader
    {
        private readonly IReadOnlyCollection<string> _defaultFiles;

        public ReadStaticFilesFromFileSystem(params string[] defaultFiles)
        {
            _defaultFiles = defaultFiles;
        }

        public Task<FileReadResult> TryRead(IDictionary<string, object> environment, string file)
        {
            var filePath = ResolvePath(environment, file);

            if (Exists(filePath))
            {
                return Task.FromResult(new FileReadResult(true, () => File.OpenRead(filePath),
                    GetCacheDataFor(filePath)));
            }

            foreach (var defaultFile in _defaultFiles)
            {
                var currentFilePath = Path.Combine(filePath, defaultFile);

                if (Exists(currentFilePath))
                {
                    return Task.FromResult(new FileReadResult(true, () => File.OpenRead(currentFilePath),
                        GetCacheDataFor(currentFilePath)));
                }
            }

            return Task.FromResult(new FileReadResult(false));
        }

        private CacheData GetCacheDataFor(string file)
        {
            //TODO:Determin this from outside somehow
            return CacheData.NotCachable();
        }

        private static bool Exists(string filePath)
        {
            return File.Exists(filePath);
        }

        private string ResolvePath(IDictionary<string, object> environment, string file)
        {
            //TODO:Correctly resolve path
            return file;
        }
    }
}