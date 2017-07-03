using System;
using System.Collections.Concurrent;
using Athena.Configuration;

namespace Athena.Logging
{
    public static class Logger
    {
        private static readonly ConcurrentBag<LogWriter> Writers = new ConcurrentBag<LogWriter>();

        public static AthenaBootstrapper LogWith(this AthenaBootstrapper bootstrapper, LogWriter logWriter)
        {
            Writers.Add(logWriter);

            return bootstrapper;
        }

        public static AthenaBootstrapper LogToConsole(this AthenaBootstrapper bootstrapper, LogLevel level = null)
        {
            return LogWith(bootstrapper, new ConsoleLogWriter(level ?? LogLevel.Info));
        }

        public static void Write(LogLevel level, string message, Exception exception = null)
        {
            foreach (var writer in Writers)
                writer.Write(level, message, exception);
        }
    }
}