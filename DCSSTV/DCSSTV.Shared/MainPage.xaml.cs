using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using FrameGenerator;
using ICSharpCode.SharpZipLib.Zip;
using Putty;
using SkiaSharp;
using SkiaSharp.Views.UWP;
using TtyRecDecoder;
#if __WASM__
using Uno.Foundation;
#endif

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DCSSTV
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private CancellationTokenSource cancellations;
        public SKBitmap skBitmap = new SKBitmap(new SKImageInfo(1602, 768));
        private MainGenerator generator;
        private DCSSReplayDriver driver;
        private TtyRecKeyframeDecoder decoder;
        public MainPage()
        {
            InitializeComponent();
            generator = new MainGenerator(new UnoFileReader(), 69);
            driver = new DCSSReplayDriver(generator, RefreshImage);
        }


        private Visibility Not(bool? value) => (!value ?? false) ? Visibility.Visible : Visibility.Collapsed;

        private void OnPaintSwapChain(object sender, SKPaintGLSurfaceEventArgs e)
        {
            if (driver.currentFrame == null) return;
            // the the canvas and properties
            var canvas = e.Surface.Canvas;

            Render(canvas, new Size(e.BackendRenderTarget.Width, e.BackendRenderTarget.Height), SKColors.Black, "SkiaSharp Red Hardware Rendering", driver.currentFrame);
        }

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            if (driver.currentFrame == null) return;
            // the the canvas and properties
            var canvas = e.Surface.Canvas;
            var info = e.Info;

            Render(canvas, new Size(info.Width, info.Height), SKColors.Black, "SkiaSharp Blue Software Rendering", driver.currentFrame);
        }

        private static void Render(SKCanvas canvas, Size size, SKColor color, string text, SKBitmap bitmap)
        {
            // get the screen density for scaling
            var display = DisplayInformation.GetForCurrentView();
            var scale = display.LogicalDpi / 96.0f;
            var scaledSize = new SKSize((float)size.Width / scale, (float)size.Height / scale);

            // handle the device screen density
            canvas.Scale(scale);

            // make sure the canvas is blank
            canvas.Clear(color);
            canvas.DrawBitmap(bitmap, 0, 0);

            // Width 41.6587026 => 144.34135
            // Height 56 => 147
        }

        private async void OnLogoButtonClicked(object sender, RoutedEventArgs e)
        {
            MainPage.FileSelectedEvent -= OnFileSelectedEvent;
            MainPage.FileSelectedEvent += OnFileSelectedEvent;
#if __WASM__
            WebAssemblyRuntime.InvokeJS("openFilePicker();");
#endif

        }

        public static void SelectFile(string imageAsDataUrl) => FileSelectedEvent?.Invoke(null, new FileSelectedEventHandlerArgs(imageAsDataUrl));

        private async void OnFileSelectedEvent(object sender, FileSelectedEventHandlerArgs e)
        {
            MainPage.FileSelectedEvent -= OnFileSelectedEvent;
            var base64Data = Regex.Match(e.FileAsDataUrl, @"data:(?<type1>.+?)/(?<type2>.+?),(?<data>.+)").Groups["data"].Value;
            var binData = Convert.FromBase64String(base64Data);
            var stream = new MemoryStream(binData);
            decoder = new TtyRecKeyframeDecoder(new List<Stream> { stream }, TimeSpan.Zero, driver.MaxDelayBetweenPackets)
            {
                PlaybackSpeed = +1,
                SeekTime = TimeSpan.Zero
            };

            await StartImageLoop();
        }

        private static event FileSelectedEventHandler FileSelectedEvent;

        private delegate void FileSelectedEventHandler(object sender, FileSelectedEventHandlerArgs args);

        private class FileSelectedEventHandlerArgs
        {
            public string FileAsDataUrl { get; }
            public FileSelectedEventHandlerArgs(string fileAsDataUrl) => FileAsDataUrl = fileAsDataUrl;

        }
  
        public async void LoadPackageFile(object sender, RoutedEventArgs e)
        {
            try
            {
                var file =
                    await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Extra.zip"));
                var bytes = await FileIO.ReadBufferAsync(file);
                var stream = bytes.AsStream();
                await ExtractExtraFileFolder(stream);
                output.Text = await FileIO.ReadTextAsync(file);
            }
            catch (Exception ex)
            {
                output.Text = ex.ToString();
            }


        }
        public async Task StartImageLoop()
        {
            var side = false;
            try
            {
                await LoadDCSSImage();

                driver.ttyrecDecoder = decoder;
                driver.PlaybackSpeed = int.Parse(speed.Text);
                driver.framerateControlTimeout = int.Parse(framerate.Text);
                await driver.StartImageGeneration();
            }
            catch (Exception ex)
            {
                output.Text = ex.ToString();
            }
            //while (int.Parse(speed.Text) >= 0)
            //{

            //    if (side)
            //    {
            //        await LoadDCSSImage();
            //    }
            //    else
            //    {
            //        await WriteAnotherImage();
            //    }

            //    side = !side;
            //    await Task.Delay(int.Parse(speed.Text));
            //}
        }

        public async Task EndImageLoop()
        {
            try
            {
                await driver.CancelImageGeneration();
            }
            catch (Exception ex)
            {
                output.Text = ex.ToString();
            }
        }


        private async Task ExtractExtraFileFolder(Stream stream)
        {
            try
            {
                var path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), @"Extra.zip");
                var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                Console.WriteLine(localFolder.Path);
                var folder = await localFolder.CreateFolderAsync("Extra", CreationCollisionOption.OpenIfExists);

                var zipInStream = new ZipInputStream(stream);
                var entry = zipInStream.GetNextEntry();
                while (entry != null && entry.CanDecompress)
                {
                    var outputFile = Path.Combine(folder.Path, entry.Name);

                    var outputDirectory = Path.GetDirectoryName(outputFile);
                    //Console.WriteLine(outputDirectory);
                    var correctFolder = await folder.CreateFolderAsync(outputDirectory, CreationCollisionOption.OpenIfExists);


                    if (entry.IsFile)
                    {

                        int size;
                        byte[] buffer = new byte[zipInStream.Length];
                        zipInStream.Read(buffer, 0, buffer.Length);
                        File.WriteAllBytes(outputFile, buffer);
                    }

                    entry = zipInStream.GetNextEntry();
                }
                zipInStream.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async void Button_Click2(object sender, RoutedEventArgs e)
        {
            await EndImageLoop();
        }
        private async void Button_Click3(object sender, RoutedEventArgs e)
        {
            MainPage.FileSelectedEvent -= OnFileSelectedEvent;
            MainPage.FileSelectedEvent += OnFileSelectedEvent;
#if __WASM__
            WebAssemblyRuntime.InvokeJS("openFilePicker();");
#endif
           
        }

        public async Task LoadDCSSImage()
        {

            await generator.InitialiseGenerator();
        }

        public void RefreshImage()
        {
            if (hwAcceleration.IsChecked.Value)
            {
                swapChain.Invalidate();
            }
            else
            {
                canvas.Invalidate();
            }
        }


    }
}