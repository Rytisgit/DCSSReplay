using System.Drawing;
using System.Windows.Forms;

namespace Window
{
    public partial class Form1 : Form
    {
        private delegate void SafeCallDelegate(Bitmap frame);
        public delegate void MyDelegate();

        public Form1()
        {
            InitializeComponent();
        }

        public void update(Bitmap frame)
        {
            if (pictureBox1.InvokeRequired)
            {
                var d = new SafeCallDelegate(update);
                pictureBox1.Invoke(d, new object[] { frame });
            }
            else
            {
                pictureBox1.Image = frame;
            }

        }
        /*
      public void find()
      {
          MyDelegate delInstatnce = new MyDelegate(abc);
          this.Invoke(delInstatnce);
      }

      public void abc()
      {
          if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
          {
              MessageBox.Show("a");
          }
      }
      */
    }
}
