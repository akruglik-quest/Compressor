using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compressor
{
    public class CompressException : Exception
    {
        public CompressException(string exMessage) : base(exMessage) { }
    }
}
