using Compressor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;


namespace GZipTest
{
    public abstract class Offsetter : IDisposable, IEnumerable<ChunkData>, IEnumerator<ChunkData>
    {
        ChunkData _current;
        readonly long _length;
        protected readonly string _filename;

        public Offsetter(string filename)
        {
            _filename = filename;
            _length = new FileInfo(filename).Length;
            _current = new ChunkData() { Number = -1 };
        }
        public virtual void Dispose() { }
        protected abstract bool GetNextChunk(ref ChunkData chunk);

        public bool MoveNext()
        {
            if (_current.Offset + _current.Length >= _length) return false;
            return GetNextChunk(ref _current);
        }

        public void Reset()
        {
            _current = new ChunkData() { Number = -1 };
        }

        IEnumerator<ChunkData> IEnumerable<ChunkData>.GetEnumerator() => this;
        public IEnumerator GetEnumerator() => this;

        ChunkData Current => _current;
        ChunkData IEnumerator<ChunkData>.Current => _current;

        object IEnumerator.Current => _current;
    }
}
