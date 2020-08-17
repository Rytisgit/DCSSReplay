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
        Font LastGdiFont;

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

        private void buttonChangeFont_Click(object sender, EventArgs e)
        {
            var dialog = new FontDialog()
            {
                AllowScriptChange = true,
                AllowSimulations = true
                ,
                AllowVectorFonts = true
                ,
                AllowVerticalFonts = true
                ,
                Font = (LastGdiFont == null) ? Configuration.Main.GdiFont : LastGdiFont
                ,
                FontMustExist = true
                ,
                ShowColor = false
            };
            var result = dialog.ShowDialog(this);
            if (result != DialogResult.OK) return;
            var font = LastGdiFont = dialog.Font;

            Size touse = new Size(0, 0);
            for (char ch = (char)0; ch < (char)255; ++ch)
            {
                if ("\u0001 \t\n\r".Contains(ch)) continue; // annoying outliers
                var m = TextRenderer.MeasureText(ch.ToString(), font, Size.Empty, TextFormatFlags.NoPadding);
                touse.Width = Math.Max(touse.Width, m.Width);
                touse.Height = Math.Max(touse.Height, m.Height);
            }

        }

        private void labelTargetChunksMemory_Click(object sender, EventArgs e)
        {

        }
    }
}
