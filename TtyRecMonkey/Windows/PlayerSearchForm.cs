using HtmlAgilityPack;
using Sample;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TtyRecMonkey
{
    public partial class  PlayerSearchForm : Form
    {
        public readonly Dictionary<string, Stream> TtyrecStreamDictionary = new Dictionary<string, Stream>();
        public MemoryStream str = new MemoryStream();
        private List<string> linkList = new List<string>();
        DataTable table = new DataTable();
        private string hostsite ;
        private string playername;
        public PlayerSearchForm()
        {
            InitializeComponent();
            table.Columns.Add("ID", typeof(Int32));
            table.Columns.Add("Date", typeof(String));
            table.Columns.Add("Progress", typeof(Int32));
            dataGridView1.AllowUserToAddRows = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == -1) MessageBox.Show("Server not selected");
            else
            {
                linkList.Clear();
                while (dataGridView1.Rows.Count > 0)
                {
                    dataGridView1.Rows.Remove(dataGridView1.Rows[0]);
                }
                playername = PlayerNametextBox.Text + '/';
                var website = hostsite + PlayerNametextBox.Text;
                if (CheckUrlExists(website) && PlayerNametextBox.Text != "")
                {
                    HtmlWeb hw = new HtmlWeb();
                    HtmlAgilityPack.HtmlDocument doc = hw.Load(website);

                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//a[@href]"))
                    {
                        string href = node.GetAttributeValue("href", null);
                        if (href.Contains("ttyrec")) linkList.Add(href);

                    }

                    int i = 0;
                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//a[text()]"))
                    {
                        if (node.InnerText.Contains("ttyrec"))
                        {
                            table.Rows.Add(i+1, node.InnerText.Split(new string[] { ".t" }, StringSplitOptions.None)[0], 0);
                            i++;
                        }
                    }
                    if (i != 0)
                    {
                        dataGridView1.DataSource = table;
                        dataGridView1.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        dataGridView1.Size = new Size((int)(ClientSize.Width - 50), (int)(ClientSize.Height - 200));
                        dataGridView1.Columns[0].Width = (int)(dataGridView1.Width * 0.1);
                        dataGridView1.Columns[1].Width = (int)(dataGridView1.Width * 0.5);
                        dataGridView1.Visible = true;
                    }
                    else
                    {
                        MessageBox.Show("No TTyRecs Found");
                    }


                }
                else
                {
                    MessageBox.Show("Not a Valid Player Name");
                }
            }

        }
        private bool CheckUrlExists(string url)
        {
            try
            {
                var request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "HEAD";
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
            catch
            {
                return false;
            }

        }

        private void PlayerSearch_Resize(object sender, EventArgs e)
        {
            if (!dataGridView1.Visible) return;
            dataGridView1.Size = new Size(ClientSize.Width - 50, ClientSize.Height - 200);
            dataGridView1.Columns[1].Width = (int)(dataGridView1.Width*0.5);
            dataGridView1.Columns[2].Width = (int)(dataGridView1.Width *0.4);
            dataGridView1.Columns[0].Width = (int)(dataGridView1.Width * 0.1);
        }

        
        public async Task DownloadFileAsync (object send, EventArgs arg)
        {
            TtyrecStreamDictionary.Clear();
            if (dataGridView1.CurrentCell != null && dataGridView1.CurrentCell.Value != null)
            {

                var href = linkList[(int)dataGridView1.CurrentRow.Cells[0].Value];
                if (href[0] == '.') href = href.Substring(2);
                var uri = href.Contains("http") ? new Uri(href) : new Uri(hostsite + playername + href);
                var wc = new WebClient();
                try
                {
                    wc.DownloadProgressChanged += (sender, e) => wc_DownloadProgressChanged(sender, e, dataGridView1.CurrentCell.RowIndex);
                    wc.DownloadDataCompleted += wc_DownloadDataCompleted;
                    await wc.DownloadDataTaskAsync(uri);
                }
                catch
                {
                    MessageBox.Show("file could not be downloaded");
                }
            }
        }

        private void wc_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            if (e.Error == null && !e.Cancelled)
            {
                string href = linkList[(int)dataGridView1.CurrentRow.Cells[0].Value];
                MessageBox.Show("Download Completed");
                str = new MemoryStream(e.Result);
                TtyrecStreamDictionary.Add(href.Split(new string[] { "." }, StringSplitOptions.None).Last(),str);
            }
            else MessageBox.Show("file could not be downloaded");
        }

        void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e, int row)
        {
            dataGridView1.Rows[row].Cells[2].Value = e.ProgressPercentage;
        }

        private void Filter_TextChanged(object sender, EventArgs e)
        {
            table.DefaultView.RowFilter = string.Format("[{0}] LIKE '%{1}%'", "Date", Filter.Text);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    hostsite = "https://underhound.eu/crawl/ttyrec/";
                    break;
                case 1:
                    hostsite = "http://crawl.akrasiac.org/rawdata/";
                    break;
                case 2:
                    hostsite = "http://crawl.berotato.org/crawl/ttyrec/";
                    break;
                case 3:
                    hostsite = "https://webzook.net/soup/ttyrecs/";
                    break;
                case 4:
                    hostsite = "https://crawl.project357.org/ttyrec/";
                    break;
                case 5:
                    hostsite = "http://crawl.develz.org/ttyrecs/";
                    break;
                case 6:
                    hostsite = "https://crawl.xtahua.com/crawl/ttyrec/";
                    break;
                case 7:
                    hostsite = "https://crawl.kelbi.org/crawl/ttyrec/";
                    break;
                case 8:
                    hostsite = "http://lazy-life.ddo.jp/mirror/ttyrecs/";
                    break;
                default:
                    break;
            }

        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            Filter.Text = dateTimePicker1.Value.Date.Date.ToShortDateString();
        }
    }
}
