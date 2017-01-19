using System;

namespace Athena.Logging
{
    public class ConsoleLogWriter : LogWriter
    {
        public void Write(LogLevel level, string message, Exception exception = null)
        {
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