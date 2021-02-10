// Copyright (c) 2010 Michael B. Edwin Rickert
//
// See the file LICENSE.txt for copying permission.

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TtyRecMonkey.Properties;

namespace TtyRecMonkey
{
    public partial class ConfigurationForm : Form
    {

        public ConfigurationForm()
        {
            InitializeComponent();

            numericUpDown1.Value = Configuration.Main.framerateControlTimeout;
            numericUpDown2.Value = Configuration.Main.TimeStepLengthMS;
            numericUpDown3.Value = Configuration.Main.MaxDelayBetweenPackets;
            radioButton1.Checked = Configuration.Main.OpenNone;
            radioButton2.Checked = Configuration.Main.OpenFileSelect;
            radioButton3.Checked = Configuration.Main.OpenDownload;

        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {

            Configuration.Main.framerateControlTimeout = (int)numericUpDown1.Value;
            Configuration.Main.TimeStepLengthMS = (int)numericUpDown2.Value;
            Configuration.Main.MaxDelayBetweenPackets = (int)numericUpDown3.Value;
            Configuration.Main.OpenNone = radioButton1.Checked;
            Configuration.Main.OpenFileSelect = radioButton2.Checked;
            Configuration.Main.OpenDownload = radioButton3.Checked;

            Configuration.Save(this);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void labelTargetChunksMemory_Click(object sender, EventArgs e)
        {

        }
    }
}
