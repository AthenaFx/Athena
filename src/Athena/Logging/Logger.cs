using System;

namespace Athena.Logging
{
    public static class Logger
    {
        private static LogWriter _logWriter = new ConsoleLogWriter();

        public static void UseLogWriter(LogWriter logWriter)
        {
            _logWriter = logWriter;
        }

        public static void Write(LogLevel level, string message, Exception exception = null)
        {
            _logWriter.Write(level, message, exception);
        }
    }
}