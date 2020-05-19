using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Putty;
using Microsoft.Extensions.Logging;
using NativeLibraryManager;
using System.Reflection;
using System.Threading;
using TtyRecDecoder;
using FrameGenerator;
using System.IO;

namespace DCSSTVBroadcaster
{
    public class Program
    {

        private const int TimeStepLengthMS = 5000;
        private readonly MainGenerator frameGenerator;
        private readonly List<DateTime> PreviousFrames = new List<DateTime>();
        private readonly Stream fileStream;
        private DateTime PreviousFrame = DateTime.Now;
        private TimeSpan MaxDelayBetweenPackets = new TimeSpan(0, 0, 0, 0, 500);//millisecondss
        private int FrameStepCount;
        private int ConsoleSwitchLevel = 1;
        public static Thread m_Thread;
        public TtyRecKeyframeDecoder ttyrecDecoder = null;
        public double PlaybackSpeed, PausedSpeed;
        public TimeSpan Seek;

        public MemoryStream destination = new MemoryStream();
        private bool run = true;


        void DoOpenFiles()
        {
            var delay = TimeSpan.Zero;

            //if (files.Length > 1)
            //{
            //    // multiselect!
            //    var fof = new FileOrderingForm(files);
            //    if (fof.ShowDialog(this) != DialogResult.OK) return;
            //    files = fof.FileOrder.ToArray();
            //    delay = TimeSpan.FromSeconds(fof.SecondsBetweenFiles);
            //}

            fileStream.CopyTo(destination);
            var streams = new List<Stream> { destination };
            ttyrecDecoder = new TtyRecKeyframeDecoder(80, 24, streams, delay, MaxDelayBetweenPackets);
            PlaybackSpeed = +1;
            Seek = TimeSpan.Zero;
        }

        //private IEnumerable<Stream> TtyrecToStream()
        //{
        //    return files.Select(f =>
        //    {

        //        if (Path.GetExtension(f) == ".ttyrec") return File.OpenRead(f);
        //        Stream streamCompressed = File.OpenRead(f);
        //        Stream streamUncompressed = new MemoryStream();
        //        if (Path.GetExtension(f) == ".bz2")
        //        {
        //            try
        //            {
        //                BZip2.Decompress(streamCompressed, streamUncompressed, false);
        //            }
        //            catch
        //            {
        //               // MessageBox.Show("The file is corrupted or not supported");
        //            }
        //            return streamUncompressed;
        //        }
        //        if (Path.GetExtension(f) == ".gz")
        //        {
        //            try
        //            {
        //                GZip.Decompress(streamCompressed, streamUncompressed, false);
        //            }
        //            catch
        //            {
        //                //MessageBox.Show("The file is corrupted or not supported");
        //            }
        //            return streamUncompressed;
        //        }
        //        return null;
        //    });
        //}
        //private IEnumerable<Stream> TtyrecToStream(Dictionary<string, Stream> s)
        //{
        //    return s.Select(f =>
        //    {

        //        if (f.Key == "ttyrec") return f.Value;
        //        Stream streamCompressed = f.Value;
        //        Stream streamUncompressed = new MemoryStream();
        //        if (f.Key == "bz2")
        //        {
        //            try
        //            {
        //                BZip2.Decompress(streamCompressed, streamUncompressed, false);
        //            }
        //            catch
        //            {
        //                MessageBox.Show("The file is corrupted or not supported");
        //            }
        //            return streamUncompressed;
        //        }
        //        if (f.Key == "gz")
        //        {
        //            try
        //            {
        //                GZip.Decompress(streamCompressed, streamUncompressed, false);
        //            }
        //            catch
        //            {
        //                MessageBox.Show("The file is corrupted or not supported");
        //            }
        //            return streamUncompressed;
        //        }
        //        return null;
        //    });
        //}

        void MainLoop()
        {
            var now = DateTime.Now;

            PreviousFrames.Add(now);
            PreviousFrames.RemoveAll(f => f.AddSeconds(1) < now);

            var dt = Math.Max(0, Math.Min(0.1, (now - PreviousFrame).TotalSeconds));
            PreviousFrame = now;

            if (ttyrecDecoder != null)
            {

                Seek += TimeSpan.FromSeconds(dt * PlaybackSpeed);

                if (Seek > ttyrecDecoder.Length)
                {
                    Seek = ttyrecDecoder.Length;
                }
                if (Seek < TimeSpan.Zero)
                {
                    Seek = TimeSpan.Zero;
                }

                if (FrameStepCount != 0)
                {
                    ttyrecDecoder.FrameStep(FrameStepCount); //step frame index by count
                    Seek = ttyrecDecoder.CurrentFrame.SinceStart;
                    FrameStepCount = 0;
                }
                else
                {

                    ttyrecDecoder.Seek(Seek);

                }

                var frame = ttyrecDecoder.CurrentFrame.Data;

                if (frame != null)
                {

                    if (!frameGenerator.isGeneratingFrame)
                    {
                        frameGenerator.isGeneratingFrame = true;
#if false
                        ThreadPool.UnsafeQueueUserWorkItem(o =>
                        {
                            try
                            {
                                bmp = frameGenerator.GenerateImage(frame, ConsoleSwitchLevel);
                                canvasView.InvalidateSurface();
                                frameGenerator.isGeneratingFrame = false;
                                frame = null;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                                //generator.GenerateImage(savedFrame);
                                frameGenerator.isGeneratingFrame = false;
                            }
                        }, null);
#else //non threaded image generation (slow)
                        //bmp = frameGenerator.GenerateImage(frame, ConsoleSwitchLevel);
                        //canvasView.InvalidateSurface();

                        frameGenerator.isGeneratingFrame = false;
#endif
                    }
                }

            }

        }
        static void Loop()
        {
            while (run)
            {
                MainLoop();
            }
        }
        static void Runner()
        {
            DoOpenFiles();
            Thread m_Thread = new Thread(() => Loop());
            m_Thread.Start();
        }

        public static void Main(string[] args)
        {
            var accessor = new ResourceAccessor(Assembly.GetExecutingAssembly());
            var libManager = new LibraryManager(
                Assembly.GetExecutingAssembly(),
                //new LibraryItem(Platform.MacOs, Bitness.x32,
                //    new LibraryFile("libPuttyDLL.dylib", accessor.Binary("libPuttyDLL.dylib"))),
                new LibraryItem(Platform.Windows, Bitness.x32,
                    new LibraryFile("libPuttyDLL.dll", accessor.Binary("libPuttyDLL.dll")))
                //,
                //new LibraryItem(Platform.Linux, Bitness.x32,
                //    new LibraryFile("libPuttyDLL.so", accessor.Binary("libPuttyDLL.so")))
                );

            libManager.LoadNativeLibrary();

            frameGenerator = new MainGenerator(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), @"Extra"));

            fileStream = file;
            Runner();
            Console.WriteLine("hah");
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
