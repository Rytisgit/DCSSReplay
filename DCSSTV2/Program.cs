using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NativeLibraryManager;

namespace DCSSTV2
{
    public class Program
    {
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
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
