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
using Windows.System;
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
        private bool readyToPlay = false;
        private int clickCount = 0;
        private bool PlaybackPaused = false;
        private int TimeStepLengthMS = 5000;
        private int FrameStepCount=0;
        private CancellationTokenSource cancellations;
        public SKBitmap skBitmap = new SKBitmap(new SKImageInfo(1602, 768));
        private MainGenerator generator;
        private DCSSReplayDriver driver;
        private TtyRecKeyframeDecoder decoder;
        private bool readyToRefresh = false;
        public MainPage()
        {
            InitializeComponent();
            generator = new MainGenerator(new UnoFileReader(), 69);
            driver = new DCSSReplayDriver(generator, RefreshImage, ReadyForRefresh);
        }

        void Grid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (decoder != null)
            {
                switch (e.Key)
                {
                    case VirtualKey.Z: driver.PlaybackSpeed = -100; break;
                    case VirtualKey.X: driver.PlaybackSpeed = -10; break;
                    case VirtualKey.C: driver.PlaybackSpeed = -1; break;
                    case VirtualKey.B: driver.PlaybackSpeed = +1; break;
                    case VirtualKey.N: driver.PlaybackSpeed = +10; break;
                    case VirtualKey.M: driver.PlaybackSpeed += +100; break;

                    case VirtualKey.F: driver.PlaybackSpeed -= 1; break;//progresive increase/decrease
                    case VirtualKey.G: driver.PlaybackSpeed += 1; break;

                    case VirtualKey.D: driver.PlaybackSpeed -= 0.2; break;//progresive increase/decrease
                    case VirtualKey.H: driver.PlaybackSpeed += 0.2; break;

                    case VirtualKey.K:
                        if (driver.PlaybackSpeed != 0) { decoder.Pause(); } //pause when frame stepping
                        FrameStepCount -= 1;//FrameStep -1 
                        break;

                    case VirtualKey.L:
                        if (driver.PlaybackSpeed != 0) { decoder.Pause(); }//pause when frame stepping
                        FrameStepCount += 1; //FrameStep +1
                        break;

                    case VirtualKey.Left:
                        driver.Seek -= driver.Seek - TimeSpan.FromMilliseconds(TimeStepLengthMS) > TimeSpan.Zero ? TimeSpan.FromMilliseconds(TimeStepLengthMS) : TimeSpan.Zero;
                        break;

                    case VirtualKey.Right:
                        driver.Seek += driver.Seek + TimeSpan.FromMilliseconds(TimeStepLengthMS) < driver.ttyrecDecoder.Length ? TimeSpan.FromMilliseconds(TimeStepLengthMS) : driver.ttyrecDecoder.Length;
                        break;

                    case VirtualKey.A:
                        driver.ConsoleSwitchLevel = driver.ConsoleSwitchLevel != 2 ? 2 : 1;//switch console and tile windows around when in normal layout mode
                        break;

                    case VirtualKey.S:
                        driver.ConsoleSwitchLevel = driver.ConsoleSwitchLevel != 3 ? 3 : 1;//switch to full console mode ound when in normal layout mode
                        break;

                    case VirtualKey.V://Play / Pause
                    case VirtualKey.Space:
                        driver.PlaybackSpeed = PlaybackPaused ? 1 : 0; 
                        PlaybackPaused = !PlaybackPaused;
                        break;
                }
              

            }

            base.OnKeyDown(e);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await SetOutputText("Navigated");
            try
            {
                await SetOutputText("Trying To Initialise Generator, Please Wait");
                await generator.InitialiseGenerator();
            }
            catch (Exception exception)
            {
                try
                {
                    await SetOutputText("Caught exception On Navigation Load, trying to Cache missing Images, Please Wait");
                    Console.WriteLine("Caught exception On Navigation Load, trying to Cache missing Images");
                    Debug.WriteLine(exception);
                    await LoadExtraFolderIndexedDB();
                    await CacheFontInIndexedDB("cour.ttf");
                    await SetOutputText("Reloading Image generator after data cache, please wait...");
                    await generator.ReinitializeGenerator();
                    Console.WriteLine("Done Loading");
                }
                catch (Exception e1)
                {
                    await SetOutputText("Something Bad Happened.");
                    Console.WriteLine(e1);
                    throw;
                }
            }
            readyToPlay = true;
            await SetOutputText("Image generator Initialized, Start Playback");
        }

        private Task SetOutputText(string text)
        {
            output.Text = text;
            return Task.CompletedTask;
        }

        private Visibility Not(bool? value) => (!value ?? false) ? Visibility.Visible : Visibility.Collapsed;

        private void OnPaintSwapChain(object sender, SKPaintGLSurfaceEventArgs e)
        {
            if (driver.currentFrame == null) return;
            // the the canvas and properties
            var canvas = e.Surface.Canvas;

            Render(canvas, new Size(e.BackendRenderTarget.Width, e.BackendRenderTarget.Height), SKColors.Black, true, driver.currentFrame);
            readyToRefresh = true;
        }
        
        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            if (driver.currentFrame == null) return;
            // the the canvas and properties
            var canvas = e.Surface.Canvas;
            var info = e.Info;

            Render(canvas, new Size(info.Width, info.Height), SKColors.Black, false, driver.currentFrame);
            readyToRefresh = true;
        }

        private static void Render(SKCanvas canvas, Size size, SKColor color, bool useOldScaling, SKBitmap bitmap)
        {
            // get the screen density for scaling
            var display = DisplayInformation.GetForCurrentView();

            var scale = display.LogicalDpi / 96.0f;

            var scaledHeight = (size.Height / scale);
            var scaledWidth = (size.Width / scale);
            int scaledBitmapWidth, scaledBitmapHeight;
            SKBitmap scaledBitmap;
            if (scaledWidth < 1602 || scaledHeight < 768)
            {
                Debug.WriteLine("rescale");
                if ((size.Width / scale) * 0.4794D > (size.Height / scale))// 768/1602 = 0.4794D
                {
                    scaledBitmapWidth = (int)(size.Height / scale * 2.0859375D);
                    scaledBitmapHeight = (int)(size.Height / scale);
                }
                else
                {
                    scaledBitmapWidth = (int)(size.Width / scale);
                    scaledBitmapHeight = (int)(size.Width / scale * 0.4794D);
                }
                scaledBitmap = new SKBitmap(new SKImageInfo(scaledBitmapWidth, scaledBitmapHeight));
                bitmap.ScalePixels(scaledBitmap, SKFilterQuality.Medium);
            }
            else
            {
                scaledBitmap = bitmap;
            }
            
            // handle the device screen density
            canvas.Scale(scale);
            
            // make sure the canvas is blank
            canvas.Clear(color);
            //draw bitmap scaled to device size, fitting to width
            canvas.DrawBitmap(scaledBitmap, 0, 0); 

            // Width 41.6587026 => 144.34135
            // Height 56 => 147
        }

        private async void Button_Click_Stop_Playback(object sender, RoutedEventArgs e)
        {
            await EndImageLoop();
        }
        private async void Button_Click_Start_Playback(object sender, RoutedEventArgs e)
        {
            if (!readyToPlay)
            {
                await SetOutputText($"Still loading, click count:{++clickCount}");
                return;
            }
            await SetOutputText("Waiting for File Selection, loading file");
            MainPage.FileSelectedEvent -= OnFileSelectedEvent;
            MainPage.FileSelectedEvent += OnFileSelectedEvent;
#if __WASM__
            WebAssemblyRuntime.InvokeJS("openFilePicker();");
#endif

        }

        public static void SelectFile(string imageAsDataUrl) => FileSelectedEvent?.Invoke(null, new FileSelectedEventHandlerArgs(imageAsDataUrl));


        private readonly Regex _fileSelect = new Regex(@"data:(?<type1>.+?)/(?<type2>.+?),(?<data>.+)", RegexOptions.Compiled);
        private async void OnFileSelectedEvent(object sender, FileSelectedEventHandlerArgs e)
        {
            await SetOutputText("File Selected, loading...");
            MainPage.FileSelectedEvent -= OnFileSelectedEvent;
            var base64Data = _fileSelect.Match(e.FileAsDataUrl).Groups["data"].Value;
            var binData = Convert.FromBase64String(base64Data);
            var stream = new MemoryStream(binData);
            decoder = new TtyRecKeyframeDecoder(80,24,new List<Stream> { stream }, TimeSpan.Zero, driver.MaxDelayBetweenPackets)
            {
                PlaybackSpeed = +1,
                SeekTime = TimeSpan.Zero
            };
            await SetOutputText("Done Loading.");
            await StartImageLoop();
        }

        private static event FileSelectedEventHandler FileSelectedEvent;

        private delegate void FileSelectedEventHandler(object sender, FileSelectedEventHandlerArgs args);

        private class FileSelectedEventHandlerArgs
        {
            public string FileAsDataUrl { get; }
            public FileSelectedEventHandlerArgs(string fileAsDataUrl) => FileAsDataUrl = fileAsDataUrl;

        }
  
        private async Task LoadExtraFolderIndexedDB()
        {
            try
            {
                var file =
                    await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Extra.zip"));
                var bytes = await FileIO.ReadBufferAsync(file);
                var stream = bytes.AsStream();
                await ExtractExtraFileFolder(stream);
                await SetOutputText("Data Cached");

            }
            catch (Exception ex)
            {
                output.Text = ex.ToString();
            }
        }

        private async Task CacheFontInIndexedDB(string name)
        {
            try
            {
                var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/" + name));
                var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                Console.WriteLine(localFolder.Path);
                var bytes = await FileIO.ReadBufferAsync(file);
                var stream = bytes.AsStream();
                File.WriteAllBytes(Path.Combine(localFolder.Path, "cour.ttf"), bytes.ToArray());
                
                await SetOutputText("Font loaded");

            }
            catch (Exception ex)
            {
                output.Text = ex.ToString();
            }
        }

        public async void LoadPackageFile(object sender, RoutedEventArgs e)
        {
            await LoadExtraFolderIndexedDB();
        }

        public async Task StartImageLoop()
        {
            try
            {
                await generator.InitialiseGenerator();
                driver.ttyrecDecoder = decoder;
                driver.PlaybackSpeed = int.Parse(speed.Text);
                driver.framerateControlTimeout = int.Parse(framerate.Text);
                readyToRefresh = true;
                await driver.StartImageGeneration();
            }
            catch (Exception ex)
            {
                output.Text = ex.ToString();
            }
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
                var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                Console.WriteLine(localFolder.Path);
                var folder = await localFolder.CreateFolderAsync("Extra", CreationCollisionOption.OpenIfExists);

                var zipInStream = new ZipInputStream(stream);
                var entry = zipInStream.GetNextEntry();
                while (entry != null && entry.CanDecompress)
                {
                    var outputFile = Path.Combine(folder.Path, entry.Name);

                    await folder.CreateFolderAsync(Path.GetDirectoryName(outputFile), CreationCollisionOption.OpenIfExists);

                    if (entry.IsFile)
                    {
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


        public bool ReadyForRefresh() => readyToRefresh;

        public void RefreshImage()
        {
            if (hwAcceleration.IsChecked.Value)
            {
                //Console.WriteLine("refresh hardware");
                readyToRefresh = false;
                swapChain.Invalidate();
                
            }
            else
            {
                //Console.WriteLine("refresh software");
                readyToRefresh = false;
                canvas.Invalidate();
            }
        }

       


        private void Button_Click_Fullscreen(object sender, RoutedEventArgs e)
        {
#if __WASM__
            WebAssemblyRuntime.InvokeJS("toggleFullScreen();");
#endif
        }

        private void Button_Click_ZoomIn(object sender, RoutedEventArgs e)
        {
#if __WASM__
            WebAssemblyRuntime.InvokeJS("viewportSet(0);");
#endif
        }

        private void Button_Click_ZoomOut(object sender, RoutedEventArgs e)
        {
#if __WASM__
            WebAssemblyRuntime.InvokeJS("viewportSet(2);");
#endif
        }
    }
}