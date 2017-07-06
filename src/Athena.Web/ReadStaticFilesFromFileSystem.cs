using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Athena.Web.Caching;

namespace Athena.Web
{
    public class ReadStaticFilesFromFileSystem : StaticFileReader
    {
        private readonly IReadOnlyCollection<string> _defaultFiles;
        private readonly Func<string, CacheData> _cacheStrategy;

        public ReadStaticFilesFromFileSystem(Func<string, CacheData> cacheStrategy, params string[] defaultFiles)
        {
            _cacheStrategy = cacheStrategy;
            _defaultFiles = defaultFiles;
        }

        public Task<FileReadResult> TryRead(IDictionary<string, object> environment, string file)
        {
            var filePath = ResolvePath(environment, file);

            if (Exists(filePath))
            {
                return Task.FromResult(new FileReadResult(true, GetContentTypeFor(filePath), () => File.OpenRead(filePath),
                    GetCacheDataFor(filePath)));
            }

            foreach (var defaultFile in _defaultFiles)
            {
                var currentFilePath = Path.Combine(filePath, defaultFile);

                if (Exists(currentFilePath))
                {
                    return Task.FromResult(new FileReadResult(true, GetContentTypeFor(filePath), () => File.OpenRead(currentFilePath),
                        GetCacheDataFor(currentFilePath)));
                }
            }

            return Task.FromResult(new FileReadResult(false, ""));
        }

        private CacheData GetCacheDataFor(string file)
        {
            return _cacheStrategy(file);
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

        private string GetContentTypeFor(string filePath)
        {
            return "text/plain";
        }
    }
}