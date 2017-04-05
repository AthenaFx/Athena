using System;

namespace Athena.Web.Sample.Home
{
    public class ThrowException
    {
        public ThrowExceptionGetResult Get()
        {
            throw new Exception("Test exception");
        }
    }

    public class ThrowExceptionGetResult
    {

    }
}