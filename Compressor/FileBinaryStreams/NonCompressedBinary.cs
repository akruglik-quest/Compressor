using System.IO;

namespace Compressor
{
    public class NonCompressedBinary : FileBinary
    {
        public NonCompressedBinary(string fileName) : base(fileName) { }

        protected override byte[] Read(BinaryReader reader, int length)
        {
            return reader.ReadBytes(length);
        }

        protected override void Write(BinaryWriter writer, byte[] b)
        {
            writer.Write(b);
        }
    }
}
