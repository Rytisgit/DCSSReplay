using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace TtyRecMonkey
{
    public partial class FileOrderingForm : Form
    {
        public FileOrderingForm(string[] filenames)
        {
            InitializeComponent();

            lbFiles.Items.Clear();
            lbFiles.Items.AddRange(filenames);
            rbOrderByName.Checked = true;
        }

        public IEnumerable<string> FileOrder { get { return lbFiles.Items.Cast<string>(); } }
        public int SecondsBetweenFiles
        {
            get
            {
                int delay;
                if (!int.TryParse(tbDelay.Text, out delay)) delay = 5;
                return delay;
            }
        }

        private void buttonPlay_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void buttonMoveFileUp_Click(object sender, EventArgs e)
        {
            var i = lbFiles.SelectedIndex;

            if (i == -1) return; // nothing selected
            if (i == 0) return; // already at the top

            var item = lbFiles.SelectedItem;
            lbFiles.Items.RemoveAt(i);
            --i;
            lbFiles.Items.Insert(i, item);
            lbFiles.SelectedIndex = i;

            rbOrderByName.Checked = rbOrderByModification.Checked = rbOrderByCreation.Checked = false;
        }

        private void buttonMoveFileDown_Click(object sender, EventArgs e)
        {
            var i = lbFiles.SelectedIndex;

            if (i == -1) return; // nothing selected
            if (i == lbFiles.Items.Count - 1) return; // already at the bottom

            var item = lbFiles.SelectedItem;
            lbFiles.Items.RemoveAt(i);
            ++i;
            lbFiles.Items.Insert(i, item);
            lbFiles.SelectedIndex = i;

            rbOrderByName.Checked = rbOrderByModification.Checked = rbOrderByCreation.Checked = false;
        }

        private void rbOrderByCreation_CheckedChanged(object sender, EventArgs e)
        {
            if (rbOrderByCreation.Checked)
            {
                var sorted = lbFiles.Items.Cast<string>().OrderBy(f => File.GetCreationTime(f)).ToArray();
                lbFiles.Items.Clear();
                lbFiles.Items.AddRange(sorted);
            }
        }

        private void rbOrderByModification_CheckedChanged(object sender, EventArgs e)
        {
            if (rbOrderByModification.Checked)
            {
                var sorted = lbFiles.Items.Cast<string>().OrderBy(f => File.GetLastWriteTime(f)).ToArray();
                lbFiles.Items.Clear();
                lbFiles.Items.AddRange(sorted);
            }
        }

        private void rbOrderByName_CheckedChanged(object sender, EventArgs e)
        {
            if (rbOrderByName.Checked)
            {
                var sorted = lbFiles.Items.Cast<string>().OrderBy(f => f).ToArray();
                lbFiles.Items.Clear();
                lbFiles.Items.AddRange(sorted);
            }
        }
    }
}
