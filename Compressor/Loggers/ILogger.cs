using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compressor
{
    public enum LogLevel
    {
        Trace,
        Error
    }

    public interface ILogger
    {
        void Log(LogLevel level, string s);
    }

    public static class LoggerEx
    {
        public static void Error(this ILogger logger, string error)
        {
            logger.Log(LogLevel.Error, error);
        }
        public static void Trace(this ILogger logger, string trace)
        {
            logger.Log(LogLevel.Trace, trace);
        }
    }
}
