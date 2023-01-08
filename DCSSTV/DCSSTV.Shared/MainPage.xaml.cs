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
using System.Timers;
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
        public SKBitmap skBitmap = new SKBitmap(new SKImageInfo(1602, 768));
        private MainGenerator generator;
        private DCSSReplayDriver driver;
        private TtyRecKeyframeDecoder decoder;
        private bool readyToRefresh = false;
        private TtyrecDownloadSelectionDialog ttyrecDownloadSelectionDialog;
        private string ttyrecUrl;
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        string proxyUrl;
        bool controlIsPressed = false;
        private DispatcherTimer _timer;

        public MainPage()
        {
            InitializeComponent();
#if __WASM__
            var href = WebAssemblyRuntime.InvokeJS("window.location.href");
            var uri = new Uri(href);
            proxyUrl = $"{uri.Scheme}://{uri.Host}:3000/";
            var queriesValues = System.Web.HttpUtility.ParseQueryString(uri.Query);
#endif
            this.LostFocus += Page_LostFocus;
            this.KeyDown += Grid_KeyDown;
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _timer.Tick += Timer_Tick;
            generator = new MainGenerator(new UnoFileReader());
            driver = new DCSSReplayDriver(generator, RefreshImage, ReadyForRefresh, UpdateSeekbar);
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            ttyrecDownloadSelectionDialog = new TtyrecDownloadSelectionDialog(PassTtyrecUrl, proxyUrl);
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
            if (!localSettings.Values.ContainsKey(SaveKeys.TileDataVersion.ToString()))
            {
                localSettings.Values[SaveKeys.TileDataVersion.ToString()] = "Classic";
                driver.VersionSwitch = "Classic";
            }
            else
            {
                driver.VersionSwitch = localSettings.Values[SaveKeys.TileDataVersion.ToString()].ToString();
            }
            this.LayoutUpdated += Focus;
        }

        void PassTtyrecUrl(string url)
        {
            ttyrecUrl = url;
        }

        void Grid_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Control)
            {
                controlIsPressed = false;
                base.OnKeyDown(e);
                return;
            }
        }
        //TODO unset when entering a new window, esc to exit windows
        async void Grid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            //if (controlIsPressed)
            //{
            switch (e.Key)
            {
                case VirtualKey.E: { await OpenSettings(); return; }
                case VirtualKey.W: { await OpenTTyrecDownloadSelection(); return; }
                case VirtualKey.Q: { await OpenTTyrecFile(); return; }
                case VirtualKey.R: break;
            }

            if (decoder != null)
            {
                switch (e.Key)
                {
                    case VirtualKey.Z: SetSpeed(-100); break;
                    case VirtualKey.X: SetSpeed(-10); break;
                    case VirtualKey.C: SetSpeed(-1); break;
                    case VirtualKey.B: SetSpeed(+1); break;
                    case VirtualKey.N: SetSpeed(+10); break;
                    case VirtualKey.M: SetSpeed(+100); break;

                    case VirtualKey.F: AdjustSpeed(-1); break;//progresive increase/decrease
                    case VirtualKey.G: AdjustSpeed(1); break;

                    case VirtualKey.D: AdjustSpeed(-0.2); break;//progresive increase/decrease
                    case VirtualKey.H: AdjustSpeed(0.2); break;

                    case VirtualKey.K:
                        FrameStep(-1);
                        break;

                    case VirtualKey.L:
                        FrameStep(1);
                        break;

                    case VirtualKey.Left:
                        TimeJumpBackwards(TimeStepLengthMS);
                        break;

                    case VirtualKey.Right:
                        TimeJumpForwards(TimeStepLengthMS);
                        break;

                    case VirtualKey.A:
                        driver.ConsoleSwitchLevel = driver.ConsoleSwitchLevel != 2 ? 2 : 1;//switch console and tile windows around when in normal layout mode
                        break;

                    case VirtualKey.S:
                        driver.ConsoleSwitchLevel = driver.ConsoleSwitchLevel != 3 ? 3 : 1;//switch to full console mode ound when in normal layout mode
                        break;

                    case VirtualKey.T:
                        SwitchTileDataVersion(driver.VersionSwitch == "Classic" ? "2023" : "Classic");//switch png version which is being used
                        break;
                    case VirtualKey.V://Play / Pause
                    case VirtualKey.Space:
                        if (driver.ttyrecDecoder.Paused) UnPause();
                        else Pause(); 
                        break;
                }
              

            }

            base.OnKeyDown(e);
        }

        private void Pause()
        {
            if (driver.ttyrecDecoder == null) return;
            driver.ttyrecDecoder.Pause();
            speedTextBlock.Text = $"Speed: {driver.ttyrecDecoder.PlaybackSpeed}";
        }

        private void UnPause()
        {
            if (driver.ttyrecDecoder == null) return;
            driver.ttyrecDecoder.Unpause();
            speedTextBlock.Text = $"Speed: {driver.ttyrecDecoder.PlaybackSpeed}";
        }

        private void SetSpeed(int speed)
        {
            if (driver.ttyrecDecoder == null) return;
            driver.ttyrecDecoder.PlaybackSpeed = speed;
            speedTextBlock.Text = $"Speed: {driver.ttyrecDecoder.PlaybackSpeed}";
        }

        private void AdjustSpeed(double speed)
        {
            if (driver.ttyrecDecoder == null) return;
            driver.ttyrecDecoder.PlaybackSpeed += speed;
            speedTextBlock.Text = $"Speed: {driver.ttyrecDecoder.PlaybackSpeed}";
        }
        private void FrameStep(int frameCount)
        {
            if (driver.ttyrecDecoder == null) return;
            Pause(); //pause when frame stepping
            driver.FrameStepCount += frameCount;
        }
        private void TimeJumpBackwards(int timeStepMiliseconds)
        {
            if (driver.ttyrecDecoder == null) return;
            driver.Seek -= driver.Seek - TimeSpan.FromMilliseconds(timeStepMiliseconds) > TimeSpan.Zero ? TimeSpan.FromMilliseconds(timeStepMiliseconds) : TimeSpan.Zero;
        }
        private void TimeJumpForwards(int timeStepMiliseconds)
        {
            if (driver.ttyrecDecoder == null) return;
            driver.Seek += driver.Seek + TimeSpan.FromMilliseconds(timeStepMiliseconds) < driver.ttyrecDecoder.Length ? TimeSpan.FromMilliseconds(timeStepMiliseconds) : driver.ttyrecDecoder.Length;
        }
        private void SwitchTileDataVersion(string version)
        {
            driver.VersionSwitch = version;
            TileDataVersionButton.Content = $"Tile Data: {version}";
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await SetOutputText("Navigated");
            try
            {
                await SetOutputText("Trying To Initialise Generator");
                await generator.InitialiseGenerator();
            }
            catch (Exception exception)
            {
                try
                {
                    await SetOutputText("Files Not Found, performing first time initialisation");
#if DEBUG
                    Console.WriteLine(exception);
#endif
                    await LoadExtraFolderIndexedDB();
                    await CacheFontInIndexedDB("cour.ttf");
                    await SetOutputText("Reinitialising Generator");
                    await generator.ReinitializeGenerator();
                }
                catch (Exception e1)
                {
                    await SetOutputText("Something Bad Happened while reinitialising, check console output.");
                    Console.WriteLine(e1);
                    throw;
                }
            }
            readyToPlay = true;
            await SetOutputText("Image generator Initialized");
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

        private void Render(SKCanvas canvas, Size size, SKColor color, bool useOldScaling, SKBitmap bitmap)
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
            await OpenTTyrecFile();

        }
        private void Timer_Tick(object sender, object e)
        {
            _timer.Stop();
            // Bring focus back to the base page
            this.Focus(FocusState.Programmatic);
        }
        private void Page_LostFocus(object sender, RoutedEventArgs e)
        {  
                _timer.Stop();
                _timer.Start();
        }
        private async Task OpenTTyrecFile()
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
                //TODO wasm multiple extra folder handling
                var file =
                    await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Extra.zip"));
                var file2 =
                    await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Extra2023.zip"));
                var bytes = await FileIO.ReadBufferAsync(file);
                var stream = bytes.AsStream();
                await ExtractExtraFileFolder(stream, "Extra");
                var bytes2 = await FileIO.ReadBufferAsync(file2);
                var stream2 = bytes2.AsStream();
                await ExtractExtraFileFolder(stream2, "Extra2023");
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
                instructions.Visibility = Not(true);
                await generator.InitialiseGenerator();
                driver.ttyrecDecoder = decoder;
                driver.ttyrecDecoder.PlaybackSpeed = 1;
                driver.framerateControlTimeout = Convert.ToInt32(localSettings.Values[SaveKeys.MinPause.ToString()].ToString());
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


        private async Task ExtractExtraFileFolder(Stream stream, string subfolderName)
        {
            try
            {
                var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                var folder = localFolder;

                var zipInStream = new ZipInputStream(stream);
                var entry = zipInStream.GetNextEntry();
                while (entry != null && entry.CanDecompress)
                {
                    var outputFile = Path.Combine(folder.Path, subfolderName, entry.Name);
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

        private void Seekbar_DragDelta(object sender, PointerRoutedEventArgs e)
        {
            //YOURE COOL
            if (decoder == null) return;
            var wasPaused = decoder.Paused;
            decoder.Pause();
            Slider slider = (Slider)sender;
            driver.Seek = TimeSpan.FromMilliseconds(slider.Value);
            if (!wasPaused) decoder.Unpause();
        }

        private void Button_Click_Fullscreen(object sender, RoutedEventArgs e)
        {
#if __WASM__
            WebAssemblyRuntime.InvokeJS("toggleFullScreen();");
#endif
        }

        private async Task Button_Click_Settings(object sender, RoutedEventArgs e)
        {
            await OpenSettings();
        }
        void Focus(object sender, object e)
        {
#if __WASM__
            WebAssemblyRuntime.InvokeJS("focusMain()");
#endif
        }
        private async Task OpenSettings()
        {
            SaveSettings settingsDialog = new SaveSettings();
            this.LostFocus -= Page_LostFocus;
            this.KeyDown -= Grid_KeyDown;
            _timer.Stop();
            ContentDialogResult result = await settingsDialog.ShowOneAtATimeAsync();

            this.LostFocus += Page_LostFocus;
            this.KeyDown += Grid_KeyDown;
            if (result == ContentDialogResult.Primary)
            {
                TimeStepLengthMS = Convert.ToInt32(localSettings.Values[SaveKeys.ArrowJump.ToString()].ToString());
                driver.VersionSwitch = localSettings.Values[SaveKeys.TileDataVersion.ToString()].ToString();
            }
#if __WASM__
            WebAssemblyRuntime.InvokeJS("focusMain()");
#endif
        }

        private async void Button_Click_TTyrecDownloadSelection(object sender, RoutedEventArgs e)
        {
            await OpenTTyrecDownloadSelection();
        }

        private async Task OpenTTyrecDownloadSelection()
        {
            this.LostFocus -= Page_LostFocus;
            this.KeyDown -= Grid_KeyDown;
            _timer.Stop();
            ContentDialogResult result = await ttyrecDownloadSelectionDialog.ShowOneAtATimeAsync();

            this.LostFocus += Page_LostFocus;
            this.KeyDown += Grid_KeyDown;
            if (result == ContentDialogResult.Primary)
            {
                OnTtyrecUrlReceived();
            }
#if __WASM__
            WebAssemblyRuntime.InvokeJS("focusMain()");
#endif
        }
        
        private async void OnTtyrecUrlReceived()
        {

            // Create an HttpClient object
            using var client = new HttpClient();
            // Send a GET request to the URL of the file
            using var response = await client.GetAsync(proxyUrl + ttyrecUrl);
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
                await SetOutputText("Failed to download ttyrec, check console output.");
                Console.WriteLine(response.ToString());
            }

        }
        private void Button_Reverse100(object sender, RoutedEventArgs e)
        {
            SetSpeed(-100);
            this.Focus(FocusState.Programmatic);
        }
        private void Button_Reverse10(object sender, RoutedEventArgs e)
        {
            SetSpeed(-10);
            this.Focus(FocusState.Programmatic);
        }
        private void Button_Reverse1(object sender, RoutedEventArgs e)
        {
            SetSpeed(-1);
            this.Focus(FocusState.Programmatic);
        }
        private void Button_Forward100(object sender, RoutedEventArgs e)
        {
            SetSpeed(100);
            this.Focus(FocusState.Programmatic);
        }
        private void Button_Forward10(object sender, RoutedEventArgs e)
        {
            SetSpeed(10);
            this.Focus(FocusState.Programmatic);
        }
        private void Button_Forward1(object sender, RoutedEventArgs e)
        {
            SetSpeed(1);
            this.Focus(FocusState.Programmatic);
        }
        private void Button_Reduce1(object sender, RoutedEventArgs e)
        {
            AdjustSpeed(-1);
            this.Focus(FocusState.Programmatic);
        }
        private void Button_Reduce02(object sender, RoutedEventArgs e)
        {
            AdjustSpeed(-0.2);
            this.Focus(FocusState.Programmatic);
        }
        private void Button_Add1(object sender, RoutedEventArgs e)
        {
            AdjustSpeed(1);
            this.Focus(FocusState.Programmatic);
        }
        private void Button_Add02(object sender, RoutedEventArgs e)
        {
            AdjustSpeed(0.2);
            this.Focus(FocusState.Programmatic);
        }
        private void Button_Click_VersionSwitch(object sender, RoutedEventArgs e)
        {
            SwitchTileDataVersion(driver.VersionSwitch == "Classic" ? "2023" : "Classic");
            this.Focus(FocusState.Programmatic);
        }
        private void Button_Click_FrameStepBackward(object sender, RoutedEventArgs e)
        {
            FrameStep(-1);
            this.Focus(FocusState.Programmatic);
        }
        private void Button_Click_FrameStepForward(object sender, RoutedEventArgs e)
        {
            FrameStep(1);
            this.Focus(FocusState.Programmatic);
        }
        private void Button_Click_TimeJumpBackwards(object sender, RoutedEventArgs e)
        {
            TimeJumpBackwards(TimeStepLengthMS);
            this.Focus(FocusState.Programmatic);
        }
        private void Button_Click_TimeJumpForwards(object sender, RoutedEventArgs e)
        {
            TimeJumpForwards(TimeStepLengthMS);
            this.Focus(FocusState.Programmatic);
        }

    }
}