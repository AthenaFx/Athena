using System;

namespace Athena.Logging
{
    public interface LogWriter
    {
        void Write(LogLevel level, string message, object data = null, Exception exception = null);
    }
}