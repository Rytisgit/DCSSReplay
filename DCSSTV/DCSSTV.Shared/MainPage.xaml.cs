using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Storage;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using FrameGenerator;
using ICSharpCode.SharpZipLib.Zip;
using SkiaSharp;
using TtyRecDecoder;
using Windows.System;
using Microsoft.UI.Xaml.Input;
using SkiaSharp.Views.Windows;
using DCSSTV.Helpers;
using DCSSTV.Pages;
using System.Net.Http;
using Windows.Storage.Pickers;
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
        private int TimeStepLengthMS = 5000;
        private int FrameStepCount=0;
        private CancellationTokenSource cancellations;
        public SKBitmap skBitmap = new SKBitmap(new SKImageInfo(1602, 768));
        private MainGenerator generator;
        private DCSSReplayDriver driver;
        private TtyRecKeyframeDecoder decoder;
        private bool readyToRefresh = false;
        private TtyrecDownloadSelectionDialog ttyrecDownloadSelectionDialog;
        private string ttyrecUrl;
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public MainPage()
        {
            InitializeComponent();
            generator = new MainGenerator(new UnoFileReader(), 69);
            driver = new DCSSReplayDriver(generator, RefreshImage, ReadyForRefresh, UpdateSeekbar);
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            ttyrecDownloadSelectionDialog = new TtyrecDownloadSelectionDialog(PassTtyrecUrl);
            if (!localSettings.Values.ContainsKey(SaveKeys.MaxPause.ToString()))
            {
                localSettings.Values[SaveKeys.MaxPause.ToString()] = "500";
                localSettings.Values[SaveKeys.ArrowJump.ToString()] = "5000";
                localSettings.Values[SaveKeys.MinPause.ToString()] = "5";
                localSettings.Values[SaveKeys.OpenOnStart.ToString()] = "None";
            }
            else
            {
                TimeStepLengthMS = Convert.ToInt32(localSettings.Values[SaveKeys.ArrowJump.ToString()].ToString());
            }
            UpdatedDateTextBlock.Visibility = Not(Environment.GetEnvironmentVariable("UPDATED_ON") == null);
            UpdatedDateTextBlock.Text = Environment.GetEnvironmentVariable("UPDATED_ON");
    }

        void PassTtyrecUrl(string url)
        {
            ttyrecUrl = url;
        }

        void Grid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (decoder != null)
            {
                switch (e.Key)
                {
                    case VirtualKey.Z: driver.ttyrecDecoder.PlaybackSpeed = -100; break;
                    case VirtualKey.X: driver.ttyrecDecoder.PlaybackSpeed = -10; break;
                    case VirtualKey.C: driver.ttyrecDecoder.PlaybackSpeed = -1; break;
                    case VirtualKey.B: driver.ttyrecDecoder.PlaybackSpeed = +1; break;
                    case VirtualKey.N: driver.ttyrecDecoder.PlaybackSpeed = +10; break;
                    case VirtualKey.M: driver.ttyrecDecoder.PlaybackSpeed += +100; break;

                    case VirtualKey.F: driver.ttyrecDecoder.PlaybackSpeed -= 1; break;//progresive increase/decrease
                    case VirtualKey.G: driver.ttyrecDecoder.PlaybackSpeed += 1; break;

                    case VirtualKey.D: driver.ttyrecDecoder.PlaybackSpeed -= 0.2; break;//progresive increase/decrease
                    case VirtualKey.H: driver.ttyrecDecoder.PlaybackSpeed += 0.2; break;

                    case VirtualKey.K:
                        if (driver.ttyrecDecoder.PlaybackSpeed != 0) { decoder.Pause(); } //pause when frame stepping
                        FrameStepCount -= 1;//FrameStep -1 
                        break;

                    case VirtualKey.L:
                        if (driver.ttyrecDecoder.PlaybackSpeed != 0) { decoder.Pause(); }//pause when frame stepping
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
                        if (driver.ttyrecDecoder.Paused) driver.ttyrecDecoder.Unpause();
                        else driver.ttyrecDecoder.Pause(); 
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

        private static Visibility Not(bool? value) => (!value ?? false) ? Visibility.Visible : Visibility.Collapsed;

        //private void OnPaintSwapChain(object sender, SKPaintGLSurfaceEventArgs e)
        //{
        //    if (driver.currentFrame == null) return;
        //    // the the canvas and properties
        //    var canvas = e.Surface.Canvas;

        //    Render(canvas, new Size(e.BackendRenderTarget.Width, e.BackendRenderTarget.Height), SKColors.Black, true, driver.currentFrame);
        //    readyToRefresh = true;
        //}
        
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
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add("*");
            var storageFile = await picker.PickSingleFileAsync();

            if (storageFile != null)
            {
                var memStream = new MemoryStream();
                using (var stream = await storageFile.OpenReadAsync())
                {
                    await stream.AsStream().CopyToAsync(memStream);
                }
                    
                memStream.Position = 0;

                driver.framerateControlTimeout = Convert.ToInt32(localSettings.Values[SaveKeys.MinPause.ToString()].ToString());
                decoder = new TtyRecKeyframeDecoder(
                    80,
                    24,
                    new List<Stream> { DCSSTV.Streams.Streams.CompressedTtyrecToStream(storageFile.Name, memStream) },
                    TimeSpan.Zero,
                    TimeSpan.FromMilliseconds(Convert.ToDouble(localSettings.Values[SaveKeys.MaxPause.ToString()].ToString())))
                {
                    PlaybackSpeed = +1,
                    SeekTime = TimeSpan.Zero
                };
                await SetOutputText("Done Loading.");
                await StartImageLoop();

            }
            else
            {
                // Did not pick any file.
            }

        }

        //public static void SelectFile(string imageAsDataUrl) => FileSelectedEvent?.Invoke(null, new FileSelectedEventHandlerArgs(imageAsDataUrl));


        //private readonly Regex _fileSelect = new Regex(@"data:(?<type1>.+?)/(?<type2>.+?),(?<data>.+)", RegexOptions.Compiled);
        //private async void OnFileSelectedEvent(object sender, FileSelectedEventHandlerArgs e)
        //{
        //    await SetOutputText("File Selected, loading...");
        //    MainPage.FileSelectedEvent -= OnFileSelectedEvent;
        //    var base64Data = _fileSelect.Match(e.FileAsDataUrl).Groups["data"].Value;
        //    var binData = Convert.FromBase64String(base64Data);
            
        //}

        //private static event FileSelectedEventHandler FileSelectedEvent;

        //private delegate void FileSelectedEventHandler(object sender, FileSelectedEventHandlerArgs args);

        //private class FileSelectedEventHandlerArgs
        //{
        //    public string FileAsDataUrl { get; }
        //    public FileSelectedEventHandlerArgs(string fileAsDataUrl) => FileAsDataUrl = fileAsDataUrl;

        //}
  
        private async Task LoadExtraFolderIndexedDB()
        {
            try
            {
                var file =
                    await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Extra.zip"));
                var bytes = await FileIO.ReadBufferAsync(file);
                var stream = bytes.AsStream();
                await ExtractExtraFileFolder(stream);
                await SetOutputText("Data Cached, reloading");

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
                driver.ttyrecDecoder.PlaybackSpeed = int.Parse(speed.Text);
                driver.framerateControlTimeout = int.Parse(framerate.Text);//check 0 for speedup
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
                var folder = localFolder;

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
            //if (hwAcceleration.IsChecked.Value)
            //{
                //Console.WriteLine("refresh hardware");
                readyToRefresh = false;
                swapChain.Invalidate();
                
            //}
            //else
            //{
            //    //Console.WriteLine("refresh software");
            //    readyToRefresh = false;
            //    canvas.Invalidate();
            //}
        }

        public void UpdateSeekbar(int currentValue, int maxValue, string remainingTime)
        {
            ViewModel.SeekbarMaxValue = maxValue;
            ViewModel.SeekbarValue = currentValue;
            ViewModel.RemainingTime = remainingTime;
        }

        private void Button_Click_PlayPause(object sender, RoutedEventArgs e)
        {
            ViewModel.SeekbarMaxValue = 55569;
        }

        private void Seekbar_DragStarting(object sender, DragEventArgs e)
        {
            //FUCK YOU
            Console.WriteLine("PAUSE");
            decoder.Pause();
        }

        private void Seekbar_DragDelta(object sender, PointerRoutedEventArgs e)
        {
            //YOURE COOL
            if (decoder == null) return;
            decoder.Pause();
            Slider slider = (Slider)sender;
            double value = slider.Value;
            driver.Seek = TimeSpan.FromMilliseconds(value);
            decoder.Unpause();
        }

        private void Seekbar_DragCompleted(object sender, DragEventArgs e)
        {
            //FUCK YOU
            decoder.Pause();
        }


        private void Button_Click_Fullscreen(object sender, RoutedEventArgs e)
        {
#if __WASM__
            WebAssemblyRuntime.InvokeJS("toggleFullScreen();");
#endif
        }

        private async Task Button_Click_Settings(object sender, RoutedEventArgs e)
        {
            SaveSettings settingsDialog = new SaveSettings();

            ContentDialogResult result = await settingsDialog.ShowOneAtATimeAsync();
            if(result == ContentDialogResult.Primary)
            {
                TimeStepLengthMS = Convert.ToInt32(ApplicationData.Current.LocalSettings.Values[SaveKeys.ArrowJump.ToString()].ToString());
            }
        }

        private async void Button_Click_TTyrecDownloadSelection(object sender, RoutedEventArgs e)
        {
            ContentDialogResult result = await ttyrecDownloadSelectionDialog.ShowOneAtATimeAsync();

            if (result == ContentDialogResult.Primary)
            {
                OnTtyrecUrlReceived();
            }
        }
        private async void OnTtyrecUrlReceived()
        {

            // Create an HttpClient object
            using var client = new HttpClient();
            // Send a GET request to the URL of the file
            using var response = await client.GetAsync(TtyrecDownloadSelectionDialog.CORS_PROXY + ttyrecUrl);
            // Check the response status code
            if (response.IsSuccessStatusCode)
            {
                // Get the file content as a stream
                MemoryStream memStream = new();
                using var stream = await response.Content.ReadAsStreamAsync();
                await stream.CopyToAsync(memStream);
                memStream.Position = 0;

                driver.framerateControlTimeout = Convert.ToInt32(localSettings.Values[SaveKeys.MinPause.ToString()].ToString());
                decoder = new TtyRecKeyframeDecoder(
                    80,
                    24,
                    new List<Stream> { Streams.Streams.CompressedTtyrecToStream(ttyrecUrl, memStream) },
                    TimeSpan.Zero,
                    TimeSpan.FromMilliseconds(Convert.ToDouble(localSettings.Values[SaveKeys.MaxPause.ToString()].ToString())))
                {
                    PlaybackSpeed = +1,
                    SeekTime = TimeSpan.Zero
                };
                await SetOutputText("Done Loading.");
                await StartImageLoop();

            }
            else
            {
                Console.WriteLine(response.ToString());
            }

        }
    }
}