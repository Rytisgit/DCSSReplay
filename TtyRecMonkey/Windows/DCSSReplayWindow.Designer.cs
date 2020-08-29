using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace DisplayWindow
{
    partial class DCSSReplayWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DCSSReplayWindow));
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.StartTimeLabel = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.PlayButton = new System.Windows.Forms.Button();
            this.EndTimeLabel = new System.Windows.Forms.Label();
            this.SeekBar = new System.Windows.Forms.Panel();
            this.directoryEntry1 = new System.DirectoryServices.DirectoryEntry();
            this.bindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox2
            // 
            this.pictureBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox2.BackColor = System.Drawing.Color.Black;
            this.pictureBox2.Location = new System.Drawing.Point(0, 0);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(1602, 774);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox2.TabIndex = 0;
            this.pictureBox2.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Black;
            this.label1.Font = new System.Drawing.Font("Courier New", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.label1.Location = new System.Drawing.Point(12, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(738, 529);
            this.label1.TabIndex = 1;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // StartTimeLabel
            // 
            this.StartTimeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.StartTimeLabel.AutoSize = true;
            this.StartTimeLabel.BackColor = System.Drawing.Color.Silver;
            this.StartTimeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.StartTimeLabel.ForeColor = System.Drawing.Color.Black;
            this.StartTimeLabel.Location = new System.Drawing.Point(53, 17);
            this.StartTimeLabel.Name = "StartTimeLabel";
            this.StartTimeLabel.Size = new System.Drawing.Size(44, 17);
            this.StartTimeLabel.TabIndex = 11;
            this.StartTimeLabel.Text = "00:00";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.Color.Silver;
            this.panel1.Controls.Add(this.PlayButton);
            this.panel1.Controls.Add(this.EndTimeLabel);
            this.panel1.Controls.Add(this.SeekBar);
            this.panel1.Controls.Add(this.StartTimeLabel);
            this.panel1.Location = new System.Drawing.Point(0, 762);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1602, 50);
            this.panel1.TabIndex = 12;
            // 
            // PlayButton
            // 
            this.PlayButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.PlayButton.BackColor = System.Drawing.Color.Transparent;
            this.PlayButton.FlatAppearance.BorderSize = 0;
            this.PlayButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PlayButton.ForeColor = System.Drawing.Color.Transparent;
            this.PlayButton.Location = new System.Drawing.Point(3, 2);
            this.PlayButton.Name = "PlayButton";
            this.PlayButton.Size = new System.Drawing.Size(47, 47);
            this.PlayButton.TabIndex = 13;
            this.PlayButton.TabStop = false;
            this.PlayButton.UseVisualStyleBackColor = false;
            this.PlayButton.Click += new System.EventHandler(this.PlayButton_Click);
            // 
            // EndTimeLabel
            // 
            this.EndTimeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.EndTimeLabel.AutoSize = true;
            this.EndTimeLabel.BackColor = System.Drawing.Color.Silver;
            this.EndTimeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.EndTimeLabel.ForeColor = System.Drawing.Color.Black;
            this.EndTimeLabel.Location = new System.Drawing.Point(1546, 17);
            this.EndTimeLabel.Name = "EndTimeLabel";
            this.EndTimeLabel.Size = new System.Drawing.Size(44, 17);
            this.EndTimeLabel.TabIndex = 13;
            this.EndTimeLabel.Text = "00:00";
            // 
            // SeekBar
            // 
            this.SeekBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SeekBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(89)))), ((int)(((byte)(89)))));
            this.SeekBar.Location = new System.Drawing.Point(118, 18);
            this.SeekBar.Name = "SeekBar";
            this.SeekBar.Size = new System.Drawing.Size(1422, 16);
            this.SeekBar.TabIndex = 14;
            this.SeekBar.Paint += new System.Windows.Forms.PaintEventHandler(this.SeekBar_Paint);
            this.SeekBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.SeekBar_MouseDown);
            this.SeekBar.MouseUp += new System.Windows.Forms.MouseEventHandler(this.SeekBar_MouseUp);
            // 
            // DCSSReplayWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1602, 811);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox2);
            this.KeyPreview = true;
            this.Name = "DCSSReplayWindow";
            this.Text = "DCSSReplay";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DCSSReplayWindow_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        public System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.BindingSource bindingSource1;
        private Label StartTimeLabel;
        private System.Windows.Forms.Panel panel1;
        private System.DirectoryServices.DirectoryEntry directoryEntry1;
        private  System.Windows.Forms.Panel SeekBar;
        private Label EndTimeLabel;
        private Button PlayButton;
       
    }
}