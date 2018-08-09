using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Common.Logging
{
    public static class Logger
    {
        static Logger()
        {
            
        }
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
            Trace.WriteLine("[DEBUG] " + log);
        }
        public static void Exception(string log, Exception ex=null)
        {
            Trace.WriteLine("*************************************");
            Trace.WriteLine("[EXCEPTION] " + log);
            Trace.WriteLine("[EXCEPTION EX] " + ex.Message);
            WriteFile($"****************************** {Environment.NewLine}[Exception] {log} {Environment.NewLine} {ex.Demystify().ToString()} {Environment.NewLine}");
        }

        private static void WriteFile(string log)
        {
            File.AppendAllText("/var/log/jadeflix.log", log);
        }
    }
}
