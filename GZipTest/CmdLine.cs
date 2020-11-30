using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compressor
{
    public static class CmdLine
    {
        public static BaseCompressInfo ExtractCmdArguments(string[] args, ILogger log)
        {
            var data = new BaseCompressInfo();
            
            data.Operation = (Operation)Enum.Parse(typeof(Operation), args[0], true);
            data.InputFileName = args[1];
            data.OutputFileName = args[2];
            if (args.Length > 3)
            {
                throw new CompressException("Too many arguments");
            }
            if (!File.Exists(data.InputFileName))
            {
                throw new CompressException($"File '{data.InputFileName}' doesn't exist.");
            }
            if (File.Exists(data.OutputFileName))
            {
                throw new CompressException($"Cann't {data.Operation.ToString().ToLower()} into existing '{data.OutputFileName}'.");
            }
            
            return data;
        }
    }
}
