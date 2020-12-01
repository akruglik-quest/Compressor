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
        static Process[] processes;
        static AutoResetEvent[] waitHandles;
        static BaseCompressInfo s_compressInfo;

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
                si.Arguments += $" {chunk.Number * Consts.ChunkSize}";
            }

            processes[i] = new Process();
            processes[i].StartInfo = si;
            processes[i].EnableRaisingEvents = true;
            processes[i].Exited += (s, e) => waitHandles[i].Set();
            processes[i].Start();
        }

        private static void InitializeWaitHandles()
        {
            var simultaniousProcessesCount = Environment.ProcessorCount;
            processes = new Process[simultaniousProcessesCount];
            waitHandles = new AutoResetEvent[simultaniousProcessesCount];

            for (int i = 0; i < simultaniousProcessesCount; i++)
            {
                waitHandles[i] = new AutoResetEvent(true);
            }
        }

        private static string CreateAndReturnLaunchDirectory()
        {
            var productDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Compressor");
            if (!Directory.Exists(productDataFolder))
            {
                Directory.CreateDirectory(productDataFolder);
            }
            var launchFolder = Path.Combine(productDataFolder, Guid.NewGuid().ToString());
            Directory.CreateDirectory(launchFolder);
            return launchFolder;
        }

        static string GetUsingMessage() => "GZipTest.exe compress/decompress [source filename] [result filename]";

        static int Main(string[] args)
        {
            var timer = new Stopwatch();
            timer.Start();

            int result = 0;
            var logger = new ConsoleLogger();
            string launchDirectory = "";
            string logFilename = "";
            try
            {
                s_compressInfo = CmdLine.ExtractCmdArguments(args, logger);
                
                launchDirectory = CreateAndReturnLaunchDirectory();
                logFilename = Path.Combine(launchDirectory, "compressor.log");
                var locFilename = Path.Combine(launchDirectory, "compressor.loc");
                File.WriteAllText(locFilename, "0");
                

                InitializeWaitHandles();

                using (var offsetter = (s_compressInfo.Operation == Operation.Compress) ?
                    (Offsetter)new NonCompressedOffsetter(s_compressInfo.InputFileName) :
                    (Offsetter)new CompressedOffsetter(s_compressInfo.InputFileName))
                {
                    foreach (ChunkData chunk in offsetter)
                    {
                        var iFree = WaitHandle.WaitAny(waitHandles, TimeSpan.FromMinutes(10));
                        if (iFree == WaitHandle.WaitTimeout)
                        {
                            throw new CompressException($"Can't find free process during 10 minutes.");
                        }
                        HandleChunkInDedicatedProcess(iFree, s_compressInfo, chunk, logFilename, locFilename);
                    }
                    WaitHandle.WaitAll(waitHandles, TimeSpan.FromMinutes(10));
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
                        logger.Error($"Some processes was hung.");
                        result = 1;
                    }
                }
                if (File.Exists(logFilename)) //used only for errors from generated processes.
                {
                    logger.Error($"Some errors was occuried during processing. See {logFilename} for more information.");
                    result = 1;
                }
                if (result == 0)
                {
                    Directory.Delete(launchDirectory, true);
                }
            }

            timer.Stop();
            logger.Trace($"It takes {timer.Elapsed.ToString(@"m\:ss\.fff")}.");
            return result;
        }
    }
}
