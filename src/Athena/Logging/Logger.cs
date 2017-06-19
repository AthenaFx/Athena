using System;

namespace Athena.Logging
{
    public static class Logger
    {
        private static LogWriter _logWriter = new ConsoleLogWriter();

        public static AthenaBootstrapper UseLogWriter(this AthenaBootstrapper bootstrapper, LogWriter logWriter)
        {
            _logWriter = logWriter;

            return bootstrapper;
        }

        public static void Write(LogLevel level, string message, Exception exception = null)
        {
            _logWriter.Write(level, message, exception);
        }
    }
}