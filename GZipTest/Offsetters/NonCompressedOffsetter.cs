using Compressor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZipTest
{
    public class NonCompressedOffsetter : Offsetter
    {
        const int c_chunkSize = 1000000; //1MB
        public NonCompressedOffsetter(string filename) : base(filename) { }

        protected override bool GetNextChunk(ref ChunkData chunk)
        {
            chunk.Number++;
            chunk.Length = c_chunkSize;
            if (chunk.Number != 0)
            {
                chunk.Offset += c_chunkSize;
            }
            return true;
        }

    }
}
