using System;
using Athena.Logging;
using EventStore.ClientAPI;

namespace Athena.EventStore
{
    public class EventStoreLog : ILogger
    {
        public void Error(string format, params object[] args)
        {
            Logger.Write(LogLevel.Error, string.Format(format, args));
        }

        public void Error(Exception ex, string format, params object[] args)
        {
            Logger.Write(LogLevel.Error, string.Format(format, args), ex);
        }

        public void Info(string format, params object[] args)
        {
            Logger.Write(LogLevel.Info, string.Format(format, args));
        }

        public void Info(Exception ex, string format, params object[] args)
        {
            Logger.Write(LogLevel.Info, string.Format(format, args), ex);
        }

        public void Debug(string format, params object[] args)
        {
            Logger.Write(LogLevel.Debug, string.Format(format, args));
        }

        public void Debug(Exception ex, string format, params object[] args)
        {
            Logger.Write(LogLevel.Debug, string.Format(format, args), ex);
        }
    }
}