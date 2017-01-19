using System;

namespace Athena.Web.Sample.Home
{
    public class Index
    {
        public IndexGetResult Get()
        {
            return new IndexGetResult("Mattias");
        }
    }

    public class IndexGetResult
    {
        public IndexGetResult(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}