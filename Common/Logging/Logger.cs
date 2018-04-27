using System.Diagnostics;

namespace Common.Logging
{
    public static class Logger
    {
        public enum LogLevelEnum
        {
            Debug = 0,
            Info = 5,
            Warning = 10,
            Error = 15
        }
        public static LogLevelEnum LogLevel { get; set; } = LogLevelEnum.Debug;
        public static void Debug(string log)
        {
            if (LogLevel >= LogLevelEnum.Debug)
            {
                Trace.WriteLine("[DEBUG] " + log);
            }
        }
    }
}
