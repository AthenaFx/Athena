using System;
using System.IO;

namespace Athena.Web
{
    public class FileReadResult
    {
        public FileReadResult(bool exists, string contentType, Func<Stream> read = null, CacheData cacheData = null)
        {
            Exists = exists;
            ContentType = contentType;
            Read = read ?? (() => new MemoryStream());
            CacheData = cacheData ?? CacheData.NotCachable();
        }

        public bool Exists { get; }
        public string ContentType { get; }
        public Func<Stream> Read { get; }
        public CacheData CacheData { get; }
    }
}