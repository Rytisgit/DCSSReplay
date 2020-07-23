using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TtyRecDecoder;

namespace TtyRecMonkey.Windows
{
    public partial class ReplayTextSearchForm : Form
    {
        DataTable table = new DataTable();
        private readonly TtyRecKeyframeDecoder ttyRecKeyframeDecoder;

        public ReplayTextSearchForm(TtyRecKeyframeDecoder ttyRecKeyframeDecoder)
        {
            InitializeComponent();
            table.Columns.Add("Search Result", typeof(String));
            table.Columns.Add("Found Text", typeof(String));
            dataGridView1.AllowUserToAddRows = false;
            this.ttyRecKeyframeDecoder = ttyRecKeyframeDecoder;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text)) MessageBox.Show("Search is empty");
            else
            {
                while (dataGridView1.Rows.Count > 0)
                {
                    dataGridView1.Rows.Remove(dataGridView1.Rows[0]);
                }
                ttyRecKeyframeDecoder.SearchPackets(textBox1.Text, 5);
                AddDataRows(ttyRecKeyframeDecoder.SearchResults);
            }
        }

        private void AddDataRows(IEnumerable<Tuple<int, string>> searchResults)
        {
            foreach (var result in searchResults)
            {
                table.Rows.Add(result.Item1, result.Item2);
            }
            dataGridView1.DataSource = table;
            dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            //dataGridView1.Size = new Size((int)(ClientSize.Width - 50), (int)(ClientSize.Height - 200));
            //dataGridView1.Columns[0].Width = (int)(dataGridView1.Width * 0.6);
            dataGridView1.Visible = true;
        }
    }
}
