using System;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using TtyRecDecoder;
using Timer = System.Timers.Timer;
namespace DisplayWindow
{

    public partial class DCSSReplayWindow : Form
    {
   
        public static Thread m_Thread;
        public  TtyRecKeyframeDecoder ttyrecDecoder = null;
        private delegate void SafeCallDelegate(Bitmap frame);
        private delegate void SafeCallDelegateTitle(string title);
        private delegate void SafeCallDelegateTitle2(string title, string title2);
        private delegate void SafeCallDelegateSeekBar(object obj, ElapsedEventArgs e);
        private delegate void SafeCallDelegateToggleControls(bool shouldShow);
        private  Timer loopTimer;  
        public bool run = true;
        public DCSSReplayWindow()
        {
            InitializeComponent();
            PlayButton.Image = Image.FromFile(@"..\..\..\Extra\pause.png");
            typeof(Panel).InvokeMember("DoubleBuffered",
            BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
            null, SeekBar, new object[] { true });
            loopTimer = new Timer();
            loopTimer.Interval = 10;
            loopTimer.Enabled = false;
            loopTimer.Elapsed += loopTimerEvent;
            loopTimer.AutoReset = true;

        }

        public void UpdateTitle(string text)
        {
            try
            {
                if (run &&   InvokeRequired)
                {
                    var d = new SafeCallDelegateTitle(UpdateTitle);
                    IAsyncResult ar = this.BeginInvoke(d, new object[] { text });
                    ar.AsyncWaitHandle.WaitOne();
                }
                else
                {
                    Text = text;
                }
            }
            catch (System.Exception)
            {
            }

        }
        public void UpdateTime(string start, string end)
        {
                if (StartTimeLabel.InvokeRequired )
                {
                var d = new SafeCallDelegateTitle2(UpdateTime);
                IAsyncResult ar2 = this.BeginInvoke(d, new object[] { start, end });
                // ar2.AsyncWaitHandle.WaitOne();
            }
                else
                {
                    if (StartTimeLabel.Text != start)
                    {
                        StartTimeLabel.Text = start;
                        EndTimeLabel.Text = end;
                        SeekBar.Invalidate();
                    }
                }
        }

        public void ShowControls(bool shouldShow)
        {
            try
            {
                if (shouldShow != label1.Visible)
                {
                    if (InvokeRequired)
                    {
                        var d = new SafeCallDelegateToggleControls(ShowControls);
                        this.Invoke(d, new object[] { shouldShow });
                    }
                    else
                    {
                        label1.Visible = shouldShow;
                    }
                }
            }
            catch (System.Exception)
            {
            }
        }

        public void Update2(Bitmap frame)
        {
            if (frame == null && pictureBox2.Image == null) return;
            pictureBox2.Image = frame;
        }

        public void SeekBar_Paint(object sender, PaintEventArgs e)
        {
            var start = ttyrecDecoder == null ? new System.TimeSpan(0) : ttyrecDecoder.CurrentFrame.SinceStart;
            var end = ttyrecDecoder == null ? new System.TimeSpan(0) : ttyrecDecoder.Length;
            var progress = start.TotalMilliseconds / end.TotalMilliseconds;
        if(progress>0)
            {
                var rect = new Rectangle(
                    e.ClipRectangle.Left,
                    e.ClipRectangle.Top, 
                    (int)(SeekBar.Width * progress),
                    SeekBar.Height
                );
                e.Graphics.DrawRectangle(Pens.DarkBlue, rect);
                e.Graphics.FillRectangle(new SolidBrush(Color.DarkBlue), rect);
            }
        }
        private  void SeekBar_MouseDown(object sender, MouseEventArgs e)
        {
            
            if (e.Button == MouseButtons.Left)
            {
                loopTimer.Enabled = true;
            }
        }
        private  void loopTimerEvent(Object source, ElapsedEventArgs e)
        {
            if (SeekBar.InvokeRequired)
            {
                var d = new SafeCallDelegateSeekBar(loopTimerEvent);
                this.Invoke(d, new object[] { source, e });
            }
            else
            {
                var MouseCoordinates = SeekBar.PointToClient(Cursor.Position);
                double progress = (double)(MouseCoordinates.X) / SeekBar.Width;
                ttyrecDecoder.SeekTime = new TimeSpan((long)(ttyrecDecoder.Length.Ticks * progress));
            }
        }

        private  void SeekBar_MouseUp(object sender, MouseEventArgs e)
        {
            loopTimer.Enabled = false;
        }
        public readonly AutoResetEvent mWaitForThread = new AutoResetEvent(false);

        private void DCSSReplayWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
           
            run = false;
            // mWaitForThread.WaitOne();
        }

        public void PlayButton_Click(object sender, System.EventArgs e)
        {
            this.ActiveControl = null;
            if (ttyrecDecoder == null) return;
            if (ttyrecDecoder.PlaybackSpeed != 0) { PlayButton.Image = Image.FromFile(@"..\..\..\Extra\play.png"); ttyrecDecoder.Pause(); }
            else { PlayButton.Image = Image.FromFile(@"..\..\..\Extra\pause.png"); ttyrecDecoder.Unpause(); }

        }

    }

}
