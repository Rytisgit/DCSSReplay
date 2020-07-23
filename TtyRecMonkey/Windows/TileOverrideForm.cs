using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TtyRecMonkey
{
    public partial class TileOverrideForm : Form
    {
        public Dictionary<string, string> tileoverides;

        public TileOverrideForm()
        {
            InitializeComponent();
            tileoverides = new Dictionary<string, string>();
            this.Visible = false;
        }

        private void TileOverrideForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Visible = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            tileoverides.Clear();
            for (int i =0; i< textBox1.Lines.Length; i++)
            {
                var text = textBox1.Lines[i];
                if (text.Contains(':'))
                {
                    var split = text.Split(':');
                    tileoverides[split[0].Replace("\\s+", "")] = split[1].Replace("\\s+", "");
                }
            }
            
        }
    }
}
