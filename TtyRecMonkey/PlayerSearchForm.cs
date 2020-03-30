using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TtyRecMonkey
{
    public partial class PlayerSearchForm : Form
    {
        private List<string> linkList;
        private string hostsite = "https://underhound.eu/crawl/ttyrec/";
        private string playername;
        public PlayerSearchForm()
        {
            InitializeComponent();
            dataGridView1.Size = new Size((int)(ClientSize.Width - 50), (int)(ClientSize.Height - 100));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            hostsite = "https://underhound.eu/crawl/ttyrec/";
            playername = PlayerNametextBox.Text+'/';
            var website = hostsite + PlayerNametextBox.Text;
            if (CheckUrlExists(website) && PlayerNametextBox.Text != "")
            {
                HtmlWeb hw = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc = hw.Load(website);
                linkList = doc.DocumentNode.SelectNodes("//a[@href]")
              .Select(i => i.GetAttributeValue("href", null)).Skip(1).ToList();
                var date = doc.DocumentNode.SelectNodes("//a[text()]")
              .Select(i => i.InnerText.Split(new string[] { ".t" }, StringSplitOptions.None)[0]).Skip(1).ToList();
               // date.RemoveAt(0);
               // linkList.RemoveAt(0);
                foreach (var l in linkList) Console.WriteLine(l);
                dataGridView1.DataSource = date.ConvertAll(x => new { Value = x }); ;
                dataGridView1.Columns[0].Width = dataGridView1.Width;
                dataGridView1.Visible = true;
            }
            else
            {
                MessageBox.Show("Not a Valid Player Name");
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
            dataGridView1.Size = new Size(ClientSize.Width - 50, ClientSize.Height - 100);
            dataGridView1.Columns[0].Width = dataGridView1.Width;

        }
        public MemoryStream str = new MemoryStream();

        public MemoryStream DownloadFile (object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.CurrentCell != null && dataGridView1.CurrentCell.Value != null)
            {
               // MessageBox.Show(hostsite + linkList[e.RowIndex]);
                WebClient wc = new WebClient();
                try
                {
                    str = new MemoryStream(wc.DownloadData(hostsite + playername + linkList[e.RowIndex]));
                //    MessageBox.Show(str.Length.ToString());
                    return str;

                }
                catch
                {
                    MessageBox.Show("file coud not be downloaded");
                    return null;
                }
            }
            else return null;

        }


    }
}
