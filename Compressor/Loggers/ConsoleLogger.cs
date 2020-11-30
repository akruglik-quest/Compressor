using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compressor
{
    public class ConsoleLogger : ILogger
    {
        public void Log(LogLevel level, string s)
        {
            Console.WriteLine($"{(level == LogLevel.Error ? "Error:" : "")}{s}");
        }
    }
}
