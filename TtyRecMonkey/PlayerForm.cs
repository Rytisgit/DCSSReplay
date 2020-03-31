// Copyright (c) 2010 Michael B. Edwin Rickert
//
// See the file LICENSE.txt for copying permission.

using FrameGenerator;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Drawing.Imaging;
using ICSharpCode.SharpZipLib.BZip2;
using DisplayWindow;

namespace TtyRecMonkey
{
    [System.ComponentModel.DesignerCategory("")]
    public class PlayerForm : Form2
    {
        private readonly MainGenerator frameGenerator;
        private TtyRecKeyframeDecoder ttyrecDecoder = null;
        private double PlaybackSpeed, PausedSpeed;
        private TimeSpan Seek;
        private readonly List<DateTime> PreviousFrames = new List<DateTime>();
        private Stream stream = new MemoryStream();
        private Bitmap bmp = new Bitmap(1602, 1050, PixelFormat.Format32bppArgb);
        private DateTime PreviousFrame = DateTime.Now;

        public PlayerForm() 
        {          
            frameGenerator = new MainGenerator();
            Visible = true;
        }

        void OpenFile()
        {
            Thread mt = new Thread(o =>
            {
                var open = new OpenFileDialog()
                {
                    CheckFileExists = true
,
                    DefaultExt = "ttyrec"
,
                    Filter = "TtyRec Files|*.ttyrec;*.bz2|All Files|*"
,
                    InitialDirectory = @"I:\home\media\ttyrecs\"
,
                    Multiselect = false
,
                    RestoreDirectory = true
,
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
        
            var streams = ttyrecToStream(files);
            ttyrecDecoder = new TtyRecKeyframeDecoder(80, 24, streams, delay);
            PlaybackSpeed = +1;
            Seek = TimeSpan.Zero;
        }
        
        private IEnumerable<Stream> ttyrecToStream(string[] files)
        {
            return files.Select(f =>
            {
                Stream stream2 = File.OpenRead(f);
                if (Path.GetExtension(f) == ".bz2")
                {
                    BZip2.Decompress(stream2, stream, false);
                    return stream;
                }
                return  stream2;
            });
        }   

        void MainLoop()
        {
            var now = DateTime.Now;
            Dictionary<int, string> myTable = new Dictionary<int, string>();
            Dictionary<int, string> myTable2 = new Dictionary<int, string>();

            PreviousFrames.Add(now);
            PreviousFrames.RemoveAll(f => f.AddSeconds(1) < now);

            var dt = Math.Max(0, Math.Min(0.1, (now - PreviousFrame).TotalSeconds));
            PreviousFrame = now;

            Seek += TimeSpan.FromSeconds(dt * PlaybackSpeed);

            if (ttyrecDecoder != null)
            {
                ttyrecDecoder.Seek(Seek);

                var frame = ttyrecDecoder.CurrentFrame.Data;

                if (frame != null)
                {

                    if (!frameGenerator.isGeneratingFrame)
                    {
                        frameGenerator.isGeneratingFrame = true;
#if true
                        ThreadPool.QueueUserWorkItem(o =>
                            {
                                try
                                {
                                    bmp = frameGenerator.GenerateImage(frame);
                                    update2(bmp);
                                    frameGenerator.isGeneratingFrame = false;
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                    //generator.GenerateImage(savedFrame);
                                    frameGenerator.isGeneratingFrame = false;
                                }
                            });
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
                var text = new[]
                    { "           PLACEHOLDER CONTROLS"
                    , ""
                    , "Ctrl+C     Reconfigure TtyRecMonkey"
                    , "Ctrl+O     Open a ttyrec"
                    , "Escape     Close ttyrec and return here"
                    , "Alt+Enter  Toggle fullscreen"
                    , ""
                    , "ZXC        Play backwards at x100, x10, or x1 speed"
                    , "   V       Play / Pause"
                    , "    BNM    Play forwards at 1x, x10, or x100 speed"
                    , ""
                    , "   F       Decrease speed by 1"
                    , "   G       Increase speed by 1"
                    , ""                    
                    , " Space     Play / Pause"
                    , ""
                    , " A / S     Zoom In/Out"
                    };

            }


            //Text = string.Format
            //    ("TtyRecMonkey -- {0} FPS -- {1} @ {2} of {3} ({4} keyframes {5} packets) -- Speed {6} -- GC recognized memory: {7}"
            //    , PreviousFrames.Count
            //    , PrettyTimeSpan(Seek)
            //    , Decoder == null ? "N/A" : PrettyTimeSpan(Decoder.CurrentFrame.SinceStart)
            //    , Decoder == null ? "N/A" : PrettyTimeSpan(Decoder.Length)
            //    , Decoder == null ? "N/A" : Decoder.Keyframes.ToString()
            //    , Decoder == null ? "N/A" : Decoder.PacketCount.ToString()
            //    , PlaybackSpeed
            //    , PrettyByteCount(GC.GetTotalMemory(false))
            //    );
          // Text = "Console";
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

                case Keys.F: PlaybackSpeed = PlaybackSpeed - 1; break;//progresive increase/decrease
                case Keys.G: PlaybackSpeed = PlaybackSpeed + 1; break;

                case Keys.D: PlaybackSpeed = PlaybackSpeed - 0.2; break;//progresive increase/decrease
                case Keys.H: PlaybackSpeed = PlaybackSpeed + 0.2; break;

                case Keys.V://Play / Pause
                case Keys.Space: 
                    if (PlaybackSpeed != 0) { PausedSpeed = PlaybackSpeed; PlaybackSpeed = 0; }
                    else { PlaybackSpeed = PausedSpeed; } 
                    break;

               // case Keys.A: ++Zoom; if (resize) ClientSize = ActiveSize; break;
              //  case Keys.S: if (Zoom > 1) --Zoom; if (resize) ClientSize = ActiveSize; break;

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
            while(true)
            {
                MainLoop();
            }
        }

        static void Main(string[] args)
        {
            using (var form = new PlayerForm())
            {
                
                if (args.Length > 0) form.DoOpenFiles(args);
                else form.OpenFile();
                Thread m_Thread = new Thread(() => form.Loop());
                m_Thread.Start();
                Application.Run(form);
            }
        }
    } 
}
