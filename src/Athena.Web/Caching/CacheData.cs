using System;

namespace Athena.Web.Caching
{
    public class CacheData
    {
        public CacheData(string cacheControl, string etag = null)
        {
            CacheControl = cacheControl;
            Etag = etag;
        }

        public string CacheControl { get; }
        public string Etag { get; }

        public static CacheData NotCachable()
        {
            return new CacheData("no-store");
        }

        public static CacheData CacheFor(TimeSpan time)
        {
            return new CacheData($"max-age={time.Seconds}");
        }

        public static CacheData UsingEtag(string etag)
        {
            return new CacheData("", etag);
        }

        public CacheData WithEtag(string etag)
        {
            return new CacheData(CacheControl, etag);
        }

        public override string ToString()
        {
            return $"{CacheControl ?? ""};{Etag ?? ""}";
        }
    }
}