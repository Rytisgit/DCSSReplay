using System.Drawing;
using System.Windows.Forms;

namespace DisplayWindow
{

    public partial class Form2 : Form
    {
        private delegate void SafeCallDelegate(Bitmap frame);
        private delegate void SafeCallDelegate2(Bitmap frame);

        public Form2()
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
        /*  private void OnResize(object sender, System.EventArgs e)
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
          }*/

        public void Update2(Bitmap frame)
        {

            pictureBox2.Image = frame;
        }
        public bool run = true;
        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            run = false;
        }
    }

}
