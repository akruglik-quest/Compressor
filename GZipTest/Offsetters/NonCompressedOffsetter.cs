﻿using Compressor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZipTest
{
    public class NonCompressedOffsetter : Offsetter
    {
        public NonCompressedOffsetter(string filename) : base(filename) { }

        protected override bool GetNextChunk(ref ChunkData chunk)
        {
            chunk.Number++;
            chunk.Length = Consts.ChunkSize;
            if (chunk.Number != 0)
            {
                chunk.Offset += Consts.ChunkSize;
            }
            return true;
        }

    }
}
