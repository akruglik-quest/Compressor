using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compressor
{
    public class CompressedBinary : FileBinary
    {
        public CompressedBinary(string fileName) : base(fileName) { }

        protected override byte[] Read(BinaryReader reader, int length)
        {
            return reader.ReadBytes(length);
        }

        protected override void Write(BinaryWriter writer, byte[] b)
        {
            byte[] len = BitConverter.GetBytes(b.Length);
            writer.Write(len);
            writer.Write(b);
        }
    }
}
