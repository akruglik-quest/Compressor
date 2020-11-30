using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compressor
{
    public class FileLogger : ILogger
    {
        string _fileName;
        long _chunkNumber; 
        public FileLogger(string fileName, long chunkNumber)
        {
            _fileName = fileName;
            _chunkNumber = chunkNumber;
        }

        public void Log(LogLevel level, string s)
        {
            var sb = new StringBuilder();
            sb.Append((DateTime.UtcNow).ToString("yyyy-MM-dd\tHH:mm:ss.ffff\t"));
            sb.AppendFormat("Px{0:X}\t", System.Diagnostics.Process.GetCurrentProcess().Id);
            sb.Append($"{_chunkNumber}\t{level}\t{s}{Environment.NewLine}");
            File.AppendAllText(_fileName, sb.ToString());
        }
    }
}
