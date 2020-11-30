using Compressor;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace GZipTest
{
    class Program
    {
        static int simultaniousProcessesCount;
        static Process[] processes;
        static AutoResetEvent[] waitHandles;
        static BaseCompressInfo s_compressInfo;

        static int FindFreeProcessNumber()
        {
            for (int i = 0; i < simultaniousProcessesCount; i++)
            {
                if (processes[i] == null || processes[i].HasExited)
                {
                    return i;
                }
            }
            return -1;
        }

        static void HandleChunkInDedicatedProcess(int i, BaseCompressInfo startData, ChunkData chunk, string logFilename, string locFilename)
        {
            var si = new ProcessStartInfo();
            si.CreateNoWindow = true;
            si.UseShellExecute = false;
            si.FileName = $"Compressor.exe";
            si.Arguments = $"{ (startData.Operation == Operation.Compress ? "compress" : "decompress")} " +
                $"\"{startData.InputFileName}\" \"{startData.OutputFileName}\" \"{logFilename}\" \"{locFilename}\" {chunk.Number} {chunk.Offset} {chunk.Length}";
            if (startData.Operation == Operation.Decompress)
            {
                si.Arguments += $" {chunk.Number * 1000000}";
            }
            Console.WriteLine(si.Arguments);

            processes[i] = new Process();
            processes[i].StartInfo = si;
            processes[i].EnableRaisingEvents = true;
            processes[i].Exited += (s, e) => waitHandles[i].Set();
            processes[i].Start();
        }

        private static (string logFilename, string locFilename) Initialize()
        {
            simultaniousProcessesCount = Environment.ProcessorCount;
            processes = new Process[simultaniousProcessesCount];
            waitHandles = new AutoResetEvent[simultaniousProcessesCount];

            for (int i = 0; i < simultaniousProcessesCount; i++)
            {
                waitHandles[i] = new AutoResetEvent(false);
            }

            var productDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Compressor");
            if (!Directory.Exists(productDataFolder))
            {
                Directory.CreateDirectory(productDataFolder);
            }
            var launchFolder = Path.Combine(productDataFolder, Guid.NewGuid().ToString());
            Directory.CreateDirectory(launchFolder);

            var res = ( logFilename : Path.Combine(launchFolder, "compressor.log"), locFilename : Path.Combine(launchFolder, "compressor.loc") );
            File.WriteAllText(res.locFilename, "0");
            return res;

        }
        static string GetUsingMessage()
        {
            return "GZipTest.exe compress/decompress [source filename] [result filename]";
        }

        static int Main(string[] args)
        {
            var timer = new Stopwatch();
            timer.Start();

            int result = 0;
            (string logFilename, string locFilename) helperFiles = ("","");
            var logger = new ConsoleLogger();
            try
            {
                s_compressInfo = CmdLine.ExtractCmdArguments(args, logger);

                helperFiles = Initialize();

                using (var offsetter = (s_compressInfo.Operation == Operation.Compress) ?
                    (Offsetter)new NonCompressedOffsetter(s_compressInfo.InputFileName) :
                    (Offsetter)new CompressedOffsetter(s_compressInfo.InputFileName))
                {
                    foreach (ChunkData chunk in offsetter)
                    {
                        int iFree = FindFreeProcessNumber();
                        if (iFree == -1)
                        {
                            iFree = WaitHandle.WaitAny(waitHandles, TimeSpan.FromMinutes(10));
                            if (iFree == WaitHandle.WaitTimeout)
                            {
                                throw new CompressException($"Can't find free process to execute during 10 minutes. See {helperFiles.logFilename} for more information.");
                            }
                        }
                        HandleChunkInDedicatedProcess(iFree, s_compressInfo, chunk, helperFiles.logFilename, helperFiles.locFilename);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                logger.Trace(GetUsingMessage());
                result = 1;
            }
            finally
            {
                if (processes != null)
                {
                    foreach (var p in processes.Where(p => p?.HasExited == false))
                    {
                        p.WaitForExit((int)TimeSpan.FromMinutes(10).TotalMilliseconds);
                    }
                }

                if (File.Exists(helperFiles.logFilename)) //used only for errors from generated processes.
                {
                    logger.Error($"Some errors was occuried during processing. See {helperFiles.logFilename} for more information.");
                    result = 1;
                }
                if (File.Exists(helperFiles.locFilename))
                {
                    File.Delete(helperFiles.locFilename);
                }
            }

            timer.Stop();
            logger.Trace($"It takes {timer.Elapsed.ToString(@"m\:ss\.fff")}.");
            return result;
        }
    }
}
