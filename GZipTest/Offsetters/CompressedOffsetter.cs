using Compressor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZipTest
{
    public class CompressedOffsetter : Offsetter
    {
        FileStream _stream;
        byte[] _bytes = new byte[4];
        public CompressedOffsetter(string filename) : base(filename)
        {
            _stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
        }

        protected override bool GetNextChunk(ref ChunkData chunk)
        {
            try
            {
                if (chunk.Number == -1)
                {
                    _stream.Seek(0, SeekOrigin.Begin);
                    chunk.Offset = 0;
                }
                else
                {
                    _stream.Seek(chunk.Length, SeekOrigin.Current);
                }
                int n = _stream.Read(_bytes, 0, 4);
                if (n != 4)
                {
                    return false;
                }
                chunk.Number++;
                chunk.Offset += chunk.Length;
                chunk.Offset += 4;
                chunk.Length = BitConverter.ToInt32(_bytes, 0);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override void Dispose()
        {
            if (_stream != null)
            {
                _stream.Dispose();
                _stream = null;
            }
        }

    }
}
