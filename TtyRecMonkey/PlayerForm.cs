// Copyright (c) 2010 Michael B. Edwin Rickert
//
// See the file LICENSE.txt for copying permission.

using DisplayWindow;
using FrameGenerator;
using ICSharpCode.SharpZipLib.BZip2;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using TtyRecDecoder;

namespace TtyRecMonkey
{
    [System.ComponentModel.DesignerCategory("")]
    public class PlayerForm : DCSSReplayWindow
    {
        private const int TimeStepLengthMS = 5000;
        private readonly MainGenerator frameGenerator;
        private TtyRecKeyframeDecoder ttyrecDecoder = null;
        private double PlaybackSpeed, PausedSpeed;
        private TimeSpan Seek;
        private readonly List<DateTime> PreviousFrames = new List<DateTime>();
        private Bitmap bmp = new Bitmap(1602, 1050, PixelFormat.Format32bppArgb);
        private DateTime PreviousFrame = DateTime.Now;
        private TimeSpan MaxDelayBetweenPackets = new TimeSpan(0,0,0,0,500);//millisecondss
        private int FrameStepCount;
        private int ConsoleSwitchLevel = 1;

        public PlayerForm()
        {
            this.Icon = Properties.Resource1.dcssreplay;
            frameGenerator = new MainGenerator();
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
                    Filter = "TtyRec Files|*.ttyrec;*.bz2|All Files|*",
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
            PlaybackSpeed = +1;
            Seek = TimeSpan.Zero;
        }

        private IEnumerable<Stream> TtyrecToStream(string[] files)
        {
            return files.Select(f =>
            {
                Stream stream2 = File.OpenRead(f);
                if (Path.GetExtension(f) == ".bz2")
                {
                    Stream stream = new MemoryStream();

                    try
                    {
                        BZip2.Decompress(stream2, stream, false);
                    }
                    catch 
                    {
                        System.Windows.Forms.MessageBox.Show("The file is corrupted or not supported");
                    }

                    return stream;
                }
                return stream2;
            });
        }

        void MainLoop()
        {
            var now = DateTime.Now;

            PreviousFrames.Add(now);
            PreviousFrames.RemoveAll(f => f.AddSeconds(1) < now);

            var dt = Math.Max(0, Math.Min(0.1, (now - PreviousFrame).TotalSeconds));
            PreviousFrame = now;

            if (ttyrecDecoder != null)
            {
                ShowControls(false);

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
#if true                
                        ThreadPool.UnsafeQueueUserWorkItem(o =>
                            {
                                try
                                {
                                    bmp = frameGenerator.GenerateImage(frame, ConsoleSwitchLevel);
                                    Update2(bmp);
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

            UpdateTitle(string.Format("DCSSReplay -- {0} FPS -- {1} @ {2} of {3} ({4} keyframes {5} packets) -- Speed {6}",
                 PreviousFrames.Count,
                 PrettyTimeSpan(Seek),
                 ttyrecDecoder == null ? "N/A" : PrettyTimeSpan(ttyrecDecoder.CurrentFrame.SinceStart),
                 ttyrecDecoder == null ? "N/A" : PrettyTimeSpan(ttyrecDecoder.Length),
                 ttyrecDecoder == null ? "N/A" : ttyrecDecoder.Keyframes.ToString(),
                 ttyrecDecoder == null ? "N/A" : ttyrecDecoder.PacketCount.ToString(),
                 PlaybackSpeed)
            );
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            //   bool resize = (WindowState == FormWindowState.Normal) && (ClientSize == ActiveSize);

            switch (e.KeyData)
            {

                case Keys.Escape: using (ttyrecDecoder) { } ttyrecDecoder = null; break;
                //case Keys.Control | Keys.C: Reconfigure(); break;
                case Keys.Control | Keys.O: OpenFile(); break;
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

                case Keys.Z: PlaybackSpeed = -100; break;
                case Keys.X: PlaybackSpeed = -10; break;
                case Keys.C: PlaybackSpeed = -1; break;
                case Keys.B: PlaybackSpeed = +1; break;
                case Keys.N: PlaybackSpeed = +10; break;
                case Keys.M: PlaybackSpeed += +100; break;

                case Keys.F: PlaybackSpeed -= 1; break;//progresive increase/decrease
                case Keys.G: PlaybackSpeed += 1; break;

                case Keys.D: PlaybackSpeed -= 0.2; break;//progresive increase/decrease
                case Keys.H: PlaybackSpeed += 0.2; break;

                case Keys.Oemcomma: 
                    if (PlaybackSpeed != 0) { PausedSpeed = PlaybackSpeed; PlaybackSpeed = 0; } //pause when frame stepping
                    FrameStepCount -= 1;//FrameStep -1 
                    break;

                case Keys.OemPeriod:
                    if (PlaybackSpeed != 0) { PausedSpeed = PlaybackSpeed; PlaybackSpeed = 0; }//pause when frame stepping
                    FrameStepCount += 1; //FrameStep +1
                    break;

                case Keys.Left:
                    Seek -= Seek - TimeSpan.FromMilliseconds(TimeStepLengthMS) > TimeSpan.Zero ? TimeSpan.FromMilliseconds(TimeStepLengthMS) : TimeSpan.Zero;
                    break;

                case Keys.Right:
                    Seek += Seek + TimeSpan.FromMilliseconds(TimeStepLengthMS) < ttyrecDecoder.Length ? TimeSpan.FromMilliseconds(TimeStepLengthMS) : ttyrecDecoder.Length;
                    break;

                case Keys.A:
                    ConsoleSwitchLevel = ConsoleSwitchLevel != 2 ? 2 : 1;//switch console and tile windows around when in normal layout mode
                    break;

                case Keys.S:
                    ConsoleSwitchLevel = ConsoleSwitchLevel != 3 ? 3 : 1;//switch to full console mode ound when in normal layout mode
                    break;

                case Keys.V://Play / Pause
                case Keys.Space:
                    if (PlaybackSpeed != 0) { PausedSpeed = PlaybackSpeed; PlaybackSpeed = 0; }
                    else { PlaybackSpeed = PausedSpeed; }
                    break;
            }
            base.OnKeyDown(e);
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

        void Loop()
        {
            while (run)
            {
                MainLoop();
            }
        }

        static void Main(string[] args)
        {
            using var form = new PlayerForm();
            if (args.Length > 0) form.DoOpenFiles(args);
            else form.OpenFile();
            Thread m_Thread = new Thread(() => form.Loop());
            m_Thread.Start();
            Application.Run(form);
        }
    }
}
