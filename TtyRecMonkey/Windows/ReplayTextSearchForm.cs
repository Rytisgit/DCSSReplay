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
        private readonly TtyRecKeyframeDecoder ttyrecDecoder;

        public ReplayTextSearchForm(TtyRecKeyframeDecoder ttyRecKeyframeDecoder)
        {
            InitializeComponent();
            table.Columns.Add("TimeStamp", typeof(String));
            table.Columns.Add("SearchResult", typeof(String));
            dataGridView1.AllowUserToAddRows = false;
            this.ttyrecDecoder = ttyRecKeyframeDecoder;
            textBox1.Focus();
        }

        private void Search()
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) || textBox1.Text.Length < 2) {
                while (dataGridView1.Rows.Count > 0)
                {
                    dataGridView1.Rows.Remove(dataGridView1.Rows[0]);
                }
                AddDataRows(new List<Tuple<int, string>> { new Tuple<int, string> (0, "Search at least 3 characters.")});
            }
            else
            {
                while (dataGridView1.Rows.Count > 0)
                {
                    dataGridView1.Rows.Remove(dataGridView1.Rows[0]);
                }
                ttyrecDecoder.SearchPackets(textBox1.Text, 5);
                if (ttyrecDecoder.SearchResults.Count() < 1)
                {
                    AddDataRows(new List<Tuple<int, string>> { new Tuple<int, string>(0, "No matches found.") });
                }
                else
                {
                    AddDataRows(ttyrecDecoder.SearchResults);
                } 
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Search();
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

        private void ReplayTextSearchForm_Enter(object sender, EventArgs e)
        {
            textBox1.Focus();
        }

        private void ReplayTextSearchForm_Activated(object sender, EventArgs e)
        {
            textBox1.Focus();
        }


        protected override void OnKeyDown(KeyEventArgs e)
        {
            //   bool resize = (WindowState == FormWindowState.Normal) && (ClientSize == ActiveSize);

            switch (e.KeyData)
            {
                case Keys.Control | Keys.F: this.Visible = false; break;
            }
            base.OnKeyDown(e);
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            ttyrecDecoder.GoToFrame(int.Parse((string)table.Rows[e.RowIndex].ItemArray[0])+1 );
        }
    }
}
