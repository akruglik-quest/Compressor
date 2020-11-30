using System.IO;

namespace Compressor
{
    public abstract class FileBinary
    {
        string _fileName;
        public FileBinary(string fileName) { _fileName = fileName; }

        public byte[] Read(long offset, int length)
        {
            using (var fileStream = new FileStream(_fileName, FileMode.Open, FileAccess.Read))
            {
                fileStream.Seek(offset, SeekOrigin.Begin);
                using (var reader = new BinaryReader(fileStream))
                {
                    return Read(reader, length);
                }
            }
        }
        public void Write(byte[] b)
        {
            using (var fileStream = new FileStream(_fileName, FileMode.Append, FileAccess.Write))
            {
                using (var writer = new BinaryWriter(fileStream))
                {
                    Write(writer, b);
                }
            }
        }

        public void Write(byte[] b, long Offset)
        {
            using (var fileStream = new FileStream(_fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write))
            {
                fileStream.Seek(Offset, SeekOrigin.Begin);
                using (var writer = new BinaryWriter(fileStream))
                {
                    Write(writer, b);
                }
            }
        }


        protected abstract byte[] Read(BinaryReader reader, int length);
        protected abstract void Write(BinaryWriter writer, byte[] b);
    }
}
