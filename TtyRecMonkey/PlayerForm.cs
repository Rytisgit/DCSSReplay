// Copyright (c) 2010 Michael B. Edwin Rickert
//
// See the file LICENSE.txt for copying permission.

using DisplayWindow;
using FrameGenerator;
using ICSharpCode.SharpZipLib.BZip2;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using TtyRecDecoder;
using System.Drawing.Imaging;
using FrameGenerator.FileReading;
using ICSharpCode.SharpZipLib.GZip;
using SkiaSharp.Views.Desktop;
using TtyRecMonkey.Windows;

namespace TtyRecMonkey
{
    [System.ComponentModel.DesignerCategory("")]
    public class PlayerForm : DCSSReplayWindow
    {
        private int TimeStepLengthMS = 5000;
        PlayerSearchForm playerSearch;
        ReplayTextSearchForm replayTextSearchForm;
        private readonly MainGenerator frameGenerator;
        private readonly List<DateTime> PreviousFrames = new List<DateTime>();
        private Bitmap bmp = new Bitmap(1602, 1050, PixelFormat.Format32bppArgb);
        private DateTime PreviousFrame = DateTime.Now;
        private TimeSpan MaxDelayBetweenPackets = new TimeSpan(0,0,0,0,500);//millisecondss
        private int FrameStepCount;
        private int ConsoleSwitchLevel = 1;
        private int framerateControlTimeout = 5;
        private TileOverrideForm tileoverrideform;

        public PlayerForm()
        { 
            this.Icon = Properties.Resource1.dcssreplay;
            frameGenerator = new MainGenerator(new ReadFromFile());
            tileoverrideform = new TileOverrideForm();
            Configuration.Load(this);
            AfterConfiguration();
            Visible = true;
        }

        void OpenFile()
        {
            
            Thread mt = new Thread(o =>
            {
                var open = new OpenFileDialog()
                {
                    CheckFileExists = true,
                    DefaultExt = "ttyrec",
                    Filter = @"TtyRec Files|*.ttyrec;*.ttyrec.bz2;*.ttyrec.gz|All Files|*",
                    Multiselect = false,
                    RestoreDirectory = true,
                    Title = "Select a TtyRec to play"
                };//TODO: bring back multiselect?

                if (open.ShowDialog() != DialogResult.OK) return;

                var files = open.FileNames;
                using (open) { }
                open = null;
                DoOpenFiles(files);
            });
            mt.SetApartmentState(ApartmentState.STA);
            mt.Start();
            mt.Join();
        }

        void DoOpenFiles(string[] files)
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
            
         
            var streams = TtyrecToStream(files);
            ttyrecDecoder = new TtyRecKeyframeDecoder(80, 24, streams, delay, MaxDelayBetweenPackets);
            ttyrecDecoder.PlaybackSpeed = +1;
            ttyrecDecoder.SeekTime = TimeSpan.Zero;
        }

        private IEnumerable<Stream> TtyrecToStream(string[] files)
        {
            return files.Select(f =>
            {
                var extension = Path.GetExtension(f).Replace(".", "");
                if (extension == "ttyrec") return File.OpenRead(f);
                Stream streamCompressed = File.OpenRead(f);
                return DecompressedStream(extension, streamCompressed);
            });
        }
        private IEnumerable<Stream> TtyrecToStream(Dictionary<string, Stream> s)
        {
            return s.Select(f =>
            {
                if (f.Key=="ttyrec") return f.Value;
                Stream streamCompressed = f.Value;
                return DecompressedStream(f.Key, streamCompressed);
            });
        }

        private static Stream DecompressedStream(string compressionType, Stream streamCompressed)
        {
            Stream streamUncompressed = new MemoryStream();
            try
            {
                switch (compressionType)
                {
                    case "bz2":
                    {
                        BZip2.Decompress(streamCompressed, streamUncompressed, false);
                        return streamUncompressed;
                    }

                    case "gz":
                    {
                        GZip.Decompress(streamCompressed, streamUncompressed, false);
                        return streamUncompressed;
                    }

                    case "xz":
                    {
                        MessageBox.Show(" .XZ is not supported, download and extract to .ttyrec before running");
                        return null;
                        }

                    default:
                        MessageBox.Show("The file is corrupted or not supported");
                        return null;
                }
            }
            catch
            {
                MessageBox.Show("The file is corrupted or not supported");
            }

            return null;
        }

        void MainLoop()
        {
            Thread.Sleep(framerateControlTimeout);
            var now = DateTime.Now;

            PreviousFrames.Add(now);
            PreviousFrames.RemoveAll(f => f.AddSeconds(1) < now);

            var dt = Math.Max(0, Math.Min(0.1, (now - PreviousFrame).TotalSeconds));
            PreviousFrame = now;

            if (ttyrecDecoder != null)
            {
                ShowControls(false);

                ttyrecDecoder.SeekTime += TimeSpan.FromSeconds(dt * ttyrecDecoder.PlaybackSpeed);

                if (ttyrecDecoder.SeekTime > ttyrecDecoder.Length)
                {
                    ttyrecDecoder.SeekTime = ttyrecDecoder.Length;
                }
                if (ttyrecDecoder.SeekTime < TimeSpan.Zero)
                {
                    ttyrecDecoder.SeekTime = TimeSpan.Zero;
                }

                if (FrameStepCount != 0)
                {
                    ttyrecDecoder.FrameStep(FrameStepCount); //step frame index by count
                    ttyrecDecoder.SeekTime = ttyrecDecoder.CurrentFrame.SinceStart;
                    FrameStepCount = 0;
                }
                else
                {

                    ttyrecDecoder.Seek(ttyrecDecoder.SeekTime);

                }

                var frame = ttyrecDecoder.CurrentFrame.Data;

                if (frame != null)
                {

                    if (!frameGenerator.isGeneratingFrame)
                    {
                        frameGenerator.isGeneratingFrame = true;
#if true                
                        ThreadPool.UnsafeQueueUserWorkItem(o =>
                            {
                                try
                                {
                                    bmp = frameGenerator.GenerateImage(frame, ConsoleSwitchLevel, tileoverrideform.tileoverides).ToBitmap();
                                    Update2(bmp);
                                    frameGenerator.isGeneratingFrame = false;
                                    frame = null;
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.StackTrace);
                                    //generator.GenerateImage(savedFrame);
                                    frameGenerator.isGeneratingFrame = false;
                                }
                            }, null);
#else //non threaded image generation (slow)
                            generator.GenerateImage(savedFrame);
                            update2(bmp);
                            frameGenerator.isGeneratingFrame = false;
#endif
                    }
                }

            }
            else
            {
                ShowControls(true);
                Update2(null);

            }
            UpdateTitle(
                $"DCSSReplay -- {PreviousFrames?.Count} " +
                $"FPS -- {(ttyrecDecoder == null ? "N/A" : PrettyTimeSpan(ttyrecDecoder.SeekTime)) } " +
                $"@ {(ttyrecDecoder == null ? "N/A" : PrettyTimeSpan(ttyrecDecoder.CurrentFrame.SinceStart))} " +
                $"of {(ttyrecDecoder == null ? "N/A" : PrettyTimeSpan(ttyrecDecoder.Length))} " +
                $"({(ttyrecDecoder == null ? "N/A" : ttyrecDecoder.Keyframes.ToString())} " +
                $"keyframes {(ttyrecDecoder == null ? "N/A" : ttyrecDecoder.PacketCount.ToString())} packets) -- Speed {ttyrecDecoder?.PlaybackSpeed}"
            );
            UpdateTime(ttyrecDecoder == null ? "N/A" : PrettyTimeSpan(ttyrecDecoder.CurrentFrame.SinceStart),
                  ttyrecDecoder == null ? "N/A" : PrettyTimeSpan(ttyrecDecoder.Length));
        }


        private void Reconfigure()
        {
            var cfg = new ConfigurationForm();
            if (cfg.ShowDialog(this) == DialogResult.OK) AfterConfiguration();
        }

        private void AfterConfiguration()
        {
            TimeStepLengthMS = Configuration.Main.TimeStepLengthMS;
            framerateControlTimeout = Configuration.Main.framerateControlTimeout;

            if (MaxDelayBetweenPackets == new TimeSpan(0, 0, 0, 0, Configuration.Main.MaxDelayBetweenPackets)) return;

            ttyrecDecoder = null;

            MaxDelayBetweenPackets = new TimeSpan(0, 0, 0, 0, Configuration.Main.MaxDelayBetweenPackets);

        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            //   bool resize = (WindowState == FormWindowState.Normal) && (ClientSize == ActiveSize);

            switch (e.KeyData)
            {

                case Keys.Escape:
                    using (ttyrecDecoder)
                    {
                    }

                    ttyrecDecoder = null;
                    break;
                case Keys.Control | Keys.G:
                    PlayerDownloadWindow();
                    break;
                //case Keys.Control | Keys.T: TileOverrideWindow(); break;
                case Keys.Control | Keys.O:
                    OpenFile();
                    break;
                case Keys.Control | Keys.F:
                    ReplayTextSearchWindow();
                    break;
                case Keys.Control | Keys.C:
                    Reconfigure();
                    break;
                case Keys.Alt | Keys.Enter:
                    if (FormBorderStyle == FormBorderStyle.None)
                    {
                        FormBorderStyle = FormBorderStyle.Sizable;
                        WindowState = FormWindowState.Normal;
                    }
                    else
                    {
                        FormBorderStyle = FormBorderStyle.None;
                        WindowState = FormWindowState.Maximized;
                    }

                    e.SuppressKeyPress = true; // also supresses annoying dings
                    break;
            }

            if (ttyrecDecoder != null)
            {
                switch (e.KeyData)
                {
                    case Keys.Z: ttyrecDecoder.PlaybackSpeed = -100; break;
                    case Keys.X: ttyrecDecoder.PlaybackSpeed = -10; break;
                    case Keys.C: ttyrecDecoder.PlaybackSpeed = -1; break;
                    case Keys.B: ttyrecDecoder.PlaybackSpeed = +1; break;
                    case Keys.N: ttyrecDecoder.PlaybackSpeed = +10; break;
                    case Keys.M: ttyrecDecoder.PlaybackSpeed += +100; break;

                    case Keys.F: ttyrecDecoder.PlaybackSpeed -= 1; break;//progresive increase/decrease
                    case Keys.G: ttyrecDecoder.PlaybackSpeed += 1; break;

                    case Keys.D: ttyrecDecoder.PlaybackSpeed -= 0.2; break;//progresive increase/decrease
                    case Keys.H: ttyrecDecoder.PlaybackSpeed += 0.2; break;

                    case Keys.Oemcomma:
                        if (ttyrecDecoder.PlaybackSpeed != 0) { ttyrecDecoder.Pause(); } //pause when frame stepping
                        FrameStepCount -= 1;//FrameStep -1 
                        break;

                    case Keys.OemPeriod:
                        if (ttyrecDecoder.PlaybackSpeed != 0) { ttyrecDecoder.Pause(); }//pause when frame stepping
                        FrameStepCount += 1; //FrameStep +1
                        break;

                    case Keys.Left:
                        ttyrecDecoder.SeekTime -= ttyrecDecoder.SeekTime - TimeSpan.FromMilliseconds(TimeStepLengthMS) > TimeSpan.Zero ? TimeSpan.FromMilliseconds(TimeStepLengthMS) : TimeSpan.Zero;
                        break;

                    case Keys.Right:
                        ttyrecDecoder.SeekTime += ttyrecDecoder.SeekTime + TimeSpan.FromMilliseconds(TimeStepLengthMS) < ttyrecDecoder.Length ? TimeSpan.FromMilliseconds(TimeStepLengthMS) : ttyrecDecoder.Length;
                        break;

                    case Keys.A:
                        ConsoleSwitchLevel = ConsoleSwitchLevel != 2 ? 2 : 1;//switch console and tile windows around when in normal layout mode
                        break;

                    case Keys.S:
                        ConsoleSwitchLevel = ConsoleSwitchLevel != 3 ? 3 : 1;//switch to full console mode ound when in normal layout mode
                        break;

                    case Keys.V://Play / Pause
                    case Keys.Space:
                        PlayButton_Click(new object(), e);
                        break;
                }

                
            }
            
            base.OnKeyDown(e);
        }

        private void TileOverrideWindow()
        {
            tileoverrideform.Visible = true;
        }

        private void PlayerDownloadWindow()
        {
            playerSearch = new PlayerSearchForm();
            playerSearch.Visible = true;
            playerSearch.dataGridView1.CellDoubleClick += DownloadTTyRec;
            playerSearch.DownloadButton.Click += DownloadTTyRec;
        }

        private void ReplayTextSearchWindow()
        {
            if (replayTextSearchForm == null || replayTextSearchForm.IsDisposed) { replayTextSearchForm = new ReplayTextSearchForm(ttyrecDecoder); }
            replayTextSearchForm.Visible = true;
            replayTextSearchForm.BringToFront();
            replayTextSearchForm.Focus();
        }



        private async void DownloadTTyRec(object sender, EventArgs e)
        {
            await playerSearch.DownloadFileAsync(sender, e); 
            var streams = TtyrecToStream(playerSearch.TtyrecStreamDictionary);

            var delay = TimeSpan.Zero;
            ttyrecDecoder = new TtyRecKeyframeDecoder(80, 24, streams, delay, MaxDelayBetweenPackets);
            ttyrecDecoder.PlaybackSpeed = +1;
            ttyrecDecoder.SeekTime = TimeSpan.Zero;
        }
   

        static string PrettyTimeSpan(TimeSpan ts)
        {
            return ts.Days == 0
                ? string.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds)
                : string.Format("{3} days, {0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Days)
                ;
        }

        static string PrettyByteCount(long bytes)
        {
            if (bytes < 10000L) return string.Format("{0:0,0}B", bytes);
            bytes /= 1000;
            if (bytes < 10000L) return string.Format("{0:0,0}KB", bytes);
            bytes /= 1000;
            if (bytes < 10000L) return string.Format("{0:0,0}MB", bytes);
            bytes /= 1000;
            if (bytes < 10000L) return string.Format("{0:0,0}GB", bytes);
            bytes /= 1000;
            if (bytes < 10000L) return string.Format("{0:0,0}TB", bytes);
            bytes /= 1000;
            return string.Format("{0:0,0}PB", bytes);
        }

        private void Loop()
        {
            while (run)
            {
                MainLoop();
            }
        }

        private static void Main(string[] args)
        {
            using var form = new PlayerForm();
            if (args.Length > 0) form.DoOpenFiles(args);
            else
            {
                if (Configuration.Main.OpenFileSelect)
                {
                    form.OpenFile();
                }
                else if (Configuration.Main.OpenDownload)
                {
                    form.PlayerDownloadWindow();
                }
            }
            Thread m_Thread = new Thread(() => form.Loop());
            m_Thread.Start();
            Application.Run(form);
            
        }
    }
}
