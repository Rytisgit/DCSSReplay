using System.Drawing;
using System.Windows.Forms;

namespace DisplayWindow
{

    public partial class DCSSReplayWindow : Form
    {
        private delegate void SafeCallDelegate(Bitmap frame);
        private delegate void SafeCallDelegate2(Bitmap frame);
        private delegate void SafeCallDelegateTitle(string title);
        private delegate void SafeCallDelegateToggleControls(bool shouldShow);

        public DCSSReplayWindow()
        {
            InitializeComponent();

        }

#pragma warning disable IDE0060 // Remove unused parameter
        public void Update(Bitmap frame)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            /* if (pictureBox1.InvokeRequired)
             {
                 var d = new SafeCallDelegate(update);
                 pictureBox1.Invoke(d, new object[] { frame });
             }
             else
             {
                 pictureBox1.Image = frame;
             }
             */
        }

        public void UpdateTitle(string text)
        {
            if (Text != text && InvokeRequired)
            {
                var d = new SafeCallDelegateTitle(UpdateTitle);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                Text = text;
            }
        }

        public void ShowControls(bool shouldShow)
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

        /* private void OnResize(object sender, System.EventArgs e)
          {
              pictureBox1.Width = (int)(0.35 * ClientSize.Width);
              pictureBox1.Height = (int)(0.3 * ClientSize.Height);
              pictureBox1.Location = new System.Drawing.Point((int)(ClientSize.Width * 0.65), (int)(0.7 * ClientSize.Height));
          }
          private void Form_Shown(Object sender, EventArgs e)
          {

              ClientSize = new Size(1602, 1050);
              pictureBox1.Width = (int)(0.35 * ClientSize.Width);
              pictureBox1.Height = (int)(0.3 * ClientSize.Height);
              pictureBox1.Location = new System.Drawing.Point((int)(ClientSize.Width * 0.65), (int)(0.7 * ClientSize.Height));
          }
          */
        public void Update2(Bitmap frame)
        {
            if (frame == null && pictureBox2.Image == null) return;
            pictureBox2.Image = frame;
        }
        public bool run = true;
        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            run = false;
        }

        private void label1_Click(object sender, System.EventArgs e)
        {

        }
    }

}
