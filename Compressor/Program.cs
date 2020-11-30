using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace Compressor
{
    class Program
    {
        static byte[] Compress(byte[] data)
        {
            using (var outputMemory = new MemoryStream())
            using (var stream = new GZipStream(outputMemory, CompressionMode.Compress))
            {
                stream.Write(data, 0, data.Length);
                stream.Close();
                return outputMemory.ToArray();
            }
        }

        static byte[] Decompress(byte[] data)
        {
            using (var inputMemory = new MemoryStream(data))
            using (var stream = new GZipStream(inputMemory, CompressionMode.Decompress))
            using (var tempStream = new MemoryStream())
            {
                stream.CopyTo(tempStream);
                return tempStream.ToArray();
            }
        }

        static void DoSychroOperation(string mutexName, Func<bool> when, Action todo)
        {
            using (var mutex = new Mutex(false, mutexName))
            {
                bool handled = false;
                while (!handled)
                {
                    try
                    {
                        mutex.WaitOne();

                        if (when())
                        {
                            todo();
                            handled = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        s_logger.Error(ex.Message);
                    }
                    finally
                    {
                        mutex.ReleaseMutex();
                    }
                }
            }
        }

        static ILogger s_logger;
        static void Main(string[] args)
        {
            s_logger = new FileLogger(args[3], Int64.Parse(args[5]));
            BaseCompressInfo data;
            string syncFilename;
            ChunkData chunk;
            long outputOffset = -1;
            try
            {
                data = new BaseCompressInfo();
                data.Operation = (Operation)Enum.Parse(typeof(Operation), args[0], true);
                data.InputFileName = args[1];
                data.OutputFileName = args[2];

                syncFilename = args[4];
                chunk.Number = Int64.Parse(args[5]);
                chunk.Offset = Int64.Parse(args[6]);
                chunk.Length = Int32.Parse(args[7]);
                if (args.Length >8)
                {
                    outputOffset = Int64.Parse(args[8]);
                }
            }
            catch (Exception ex)
            {
                s_logger.Error(ex.Message);
                return;
            }

            FileBinary input;
            FileBinary output;
            Func<byte[], byte[]> transform;

            if (data.Operation == Operation.Compress)
            {
                input = new NonCompressedBinary(data.InputFileName);
                output = new CompressedBinary(data.OutputFileName);
                transform = Compress;
            }
            else
            {
                input = new CompressedBinary(data.InputFileName);
                output = new NonCompressedBinary(data.OutputFileName);

                transform = Decompress;
            }

            try
            {
                var temp = input.Read(chunk.Offset, chunk.Length);
                var res = transform(temp);

                if (outputOffset != -1)
                {
                    output.Write(res, outputOffset);
                }
                else
                {
                    var mutexName = syncFilename.Replace('\\', '_').Replace(':', '_');
                    DoSychroOperation(mutexName,
                        () => File.ReadAllText(syncFilename) == chunk.Number.ToString(),
                        () =>
                        {
                            output.Write(res);
                            File.WriteAllText(syncFilename, (chunk.Number + 1).ToString());
                        });
                }
            }
            catch(Exception ex)
            {
                s_logger.Error(ex.Message);
            }
        }
    }
}

