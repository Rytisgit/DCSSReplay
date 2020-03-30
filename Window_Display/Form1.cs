using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Window_Display
{
    public partial class Form1 : Form
    {
        private delegate void SafeCallDelegate(Bitmap frame);
        public Form1()
        {
            InitializeComponent();
        }
        public void update2(Bitmap frame)
        {
            if (pictureBox1.InvokeRequired)
            {
                var d = new SafeCallDelegate(update2);
                pictureBox1.Invoke(d, new object[] { frame });
            }
            else
            {
                pictureBox1.Image = frame;
            }
        }
      

        private void button1_Click_1(object sender, EventArgs e)
        {
            Bitmap bit = new Bitmap(100, 100);
            Graphics g = Graphics.FromImage(bit);
            g.Clear(Color.Red);
            pictureBox1.Image = bit;
        }
    }
}
