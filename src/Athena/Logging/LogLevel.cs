namespace Athena.Logging
{
    public class LogLevel
    {
        private LogLevel(int level, string name)
        {
            Level = level;
            Name = name;
        }

        public int Level { get; }
        public string Name { get; }

        public static LogLevel Debug = new LogLevel(1, "debug");
        public static LogLevel Trace = new LogLevel(2, "trace");
        public static LogLevel Info = new LogLevel(3, "info");
        public static LogLevel Warn = new LogLevel(4, "warn");
        public static LogLevel Error = new LogLevel(5, "error");
        public static LogLevel Fatal = new LogLevel(6, "fatal");

        public override bool Equals(object obj)
        {
            var logLevel = obj as LogLevel;

            return logLevel != null && logLevel.Level == Level;
        }

        public override int GetHashCode()
        {
            return (Name ?? "").GetHashCode();
        }

        public static implicit operator int(LogLevel item)
        {
            return item?.Level ?? 0;
        }

        public static implicit operator string(LogLevel item)
        {
            return item?.Name ?? "";
        }

        public override string ToString()
        {
            return Name;
        }
    }
}