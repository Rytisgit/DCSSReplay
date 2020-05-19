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


       

        public static void Main(string[] args)
        {
            var accessor = new ResourceAccessor(Assembly.GetExecutingAssembly());
            var libManager = new LibraryManager(
                Assembly.GetExecutingAssembly(),
                //new LibraryItem(Platform.MacOs, Bitness.x32,
                //    new LibraryFile("libPuttyDLL.dylib", accessor.Binary("libPuttyDLL.dylib"))),
                new LibraryItem(Platform.Windows, Bitness.x64,
                    new LibraryFile("libPuttyDLL.dll", accessor.Binary("libPuttyDLL.dll")))
                //,
                //new LibraryItem(Platform.Linux, Bitness.x32,
                //    new LibraryFile("libPuttyDLL.so", accessor.Binary("libPuttyDLL.so")))
                );

            libManager.LoadNativeLibrary();

            //frameGenerator = new MainGenerator(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), @"Extra"));

            var term = new Putty.Terminal(80, 24);
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
