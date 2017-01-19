using System;

namespace Athena.Logging
{
    public interface LogWriter
    {
        void Write(LogLevel level, string message, Exception exception = null);
    }
}