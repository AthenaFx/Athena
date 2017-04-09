using System;
using System.Collections.Generic;
using System.IO;
using Athena.Routing;
using Athena.Web.Caching;

namespace Athena.Web.Routing
{
    public class StaticFileRouterResult : RouterResult
    {
        public StaticFileRouterResult(string contentType, Func<Stream> read, CacheData cacheData)
        {
            ContentType = contentType;
            Read = read;
            CacheData = cacheData;
        }

        public string ContentType { get; }
        public Func<Stream> Read { get; }
        public CacheData CacheData { get; }

        public IReadOnlyDictionary<string, object> GetParameters()
        {
            return new Dictionary<string, object>();
        }
    }
}