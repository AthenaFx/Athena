using System;

namespace Athena.Logging
{
    public class ConsoleLogWriter : LogWriter
    {
        private readonly LogLevel _level;

        public ConsoleLogWriter(LogLevel level)
        {
            _level = level;
        }

        public void Write(LogLevel level, string message, object data = null, Exception exception = null)
        {
            if(level.Level < _level.Level)
                return;
            
            Console.WriteLine($"[{level}] {message}");

            var lastException = exception;

            while (lastException != null)
            {
                Console.WriteLine(lastException.Message);

                lastException = lastException.InnerException;
            }
        }
    }
}