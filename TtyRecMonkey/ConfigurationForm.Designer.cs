namespace TtyRecMonkey {
	partial class ConfigurationForm {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing ) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
            this.checkBoxForceGC = new System.Windows.Forms.CheckBox();
            this.labelTargetChunksMemory = new System.Windows.Forms.Label();
            this.textBoxTargetChunksMemory = new System.Windows.Forms.TextBox();
            this.labelTargetLoadMS = new System.Windows.Forms.Label();
            this.textBoxTargetLoadMS = new System.Windows.Forms.TextBox();
            this.labelFontOverlapXY = new System.Windows.Forms.Label();
            this.textBoxFontOverlapXY = new System.Windows.Forms.TextBox();
            this.labelFont = new System.Windows.Forms.Label();
            this.buttonChangeFont = new System.Windows.Forms.Button();
            this.pictureBoxFontPreview = new System.Windows.Forms.PictureBox();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonChangeFontBuiltin1 = new System.Windows.Forms.Button();
            this.buttonChangeFontBuiltin2 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxFontPreview)).BeginInit();
            this.SuspendLayout();
            // 
            // checkBoxForceGC
            // 
            this.checkBoxForceGC.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBoxForceGC.Location = new System.Drawing.Point(12, 12);
            this.checkBoxForceGC.Name = "checkBoxForceGC";
            this.checkBoxForceGC.Size = new System.Drawing.Size(300, 21);
            this.checkBoxForceGC.TabIndex = 0;
            this.checkBoxForceGC.Text = "Force GC on unload (bad idea!)";
            this.checkBoxForceGC.UseVisualStyleBackColor = true;
            // 
            // labelTargetChunksMemory
            // 
            this.labelTargetChunksMemory.AutoSize = true;
            this.labelTargetChunksMemory.Location = new System.Drawing.Point(12, 42);
            this.labelTargetChunksMemory.Name = "labelTargetChunksMemory";
            this.labelTargetChunksMemory.Size = new System.Drawing.Size(145, 13);
            this.labelTargetChunksMemory.TabIndex = 5;
            this.labelTargetChunksMemory.Text = "Target Chunks Memory (MB):";
            // 
            // textBoxTargetChunksMemory
            // 
            this.textBoxTargetChunksMemory.Location = new System.Drawing.Point(212, 39);
            this.textBoxTargetChunksMemory.Name = "textBoxTargetChunksMemory";
            this.textBoxTargetChunksMemory.Size = new System.Drawing.Size(100, 20);
            this.textBoxTargetChunksMemory.TabIndex = 6;
            // 
            // labelTargetLoadMS
            // 
            this.labelTargetLoadMS.AutoSize = true;
            this.labelTargetLoadMS.Location = new System.Drawing.Point(12, 65);
            this.labelTargetLoadMS.Name = "labelTargetLoadMS";
            this.labelTargetLoadMS.Size = new System.Drawing.Size(159, 13);
            this.labelTargetLoadMS.TabIndex = 7;
            this.labelTargetLoadMS.Text = "Target Load Time (milliseconds):";
            // 
            // textBoxTargetLoadMS
            // 
            this.textBoxTargetLoadMS.Location = new System.Drawing.Point(212, 65);
            this.textBoxTargetLoadMS.Name = "textBoxTargetLoadMS";
            this.textBoxTargetLoadMS.Size = new System.Drawing.Size(100, 20);
            this.textBoxTargetLoadMS.TabIndex = 8;
            // 
            // labelFontOverlapXY
            // 
            this.labelFontOverlapXY.AutoSize = true;
            this.labelFontOverlapXY.Location = new System.Drawing.Point(12, 94);
            this.labelFontOverlapXY.Name = "labelFontOverlapXY";
            this.labelFontOverlapXY.Size = new System.Drawing.Size(106, 13);
            this.labelFontOverlapXY.TabIndex = 9;
            this.labelFontOverlapXY.Text = "Font Overlap (pixels):";
            // 
            // textBoxFontOverlapXY
            // 
            this.textBoxFontOverlapXY.Location = new System.Drawing.Point(212, 91);
            this.textBoxFontOverlapXY.Name = "textBoxFontOverlapXY";
            this.textBoxFontOverlapXY.Size = new System.Drawing.Size(100, 20);
            this.textBoxFontOverlapXY.TabIndex = 10;
            // 
            // labelFont
            // 
            this.labelFont.AutoSize = true;
            this.labelFont.Location = new System.Drawing.Point(12, 122);
            this.labelFont.Name = "labelFont";
            this.labelFont.Size = new System.Drawing.Size(31, 13);
            this.labelFont.TabIndex = 11;
            this.labelFont.Text = "Font:";
            // 
            // buttonChangeFont
            // 
            this.buttonChangeFont.Location = new System.Drawing.Point(212, 117);
            this.buttonChangeFont.Name = "buttonChangeFont";
            this.buttonChangeFont.Size = new System.Drawing.Size(100, 23);
            this.buttonChangeFont.TabIndex = 14;
            this.buttonChangeFont.Text = "Change Font";
            this.buttonChangeFont.UseVisualStyleBackColor = true;
            this.buttonChangeFont.Click += new System.EventHandler(this.buttonChangeFont_Click);
            // 
            // pictureBoxFontPreview
            // 
            this.pictureBoxFontPreview.Location = new System.Drawing.Point(12, 146);
            this.pictureBoxFontPreview.Name = "pictureBoxFontPreview";
            this.pictureBoxFontPreview.Size = new System.Drawing.Size(300, 300);
            this.pictureBoxFontPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBoxFontPreview.TabIndex = 9;
            this.pictureBoxFontPreview.TabStop = false;
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(12, 452);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(75, 23);
            this.buttonSave.TabIndex = 15;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(237, 452);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 16;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonChangeFontBuiltin1
            // 
            this.buttonChangeFontBuiltin1.AutoSize = true;
            this.buttonChangeFontBuiltin1.Location = new System.Drawing.Point(154, 117);
            this.buttonChangeFontBuiltin1.Name = "buttonChangeFontBuiltin1";
            this.buttonChangeFontBuiltin1.Size = new System.Drawing.Size(23, 23);
            this.buttonChangeFontBuiltin1.TabIndex = 12;
            this.buttonChangeFontBuiltin1.Text = "1";
            this.buttonChangeFontBuiltin1.UseVisualStyleBackColor = true;
            this.buttonChangeFontBuiltin1.Click += new System.EventHandler(this.buttonChangeFontBuiltin1_Click);
            // 
            // buttonChangeFontBuiltin2
            // 
            this.buttonChangeFontBuiltin2.AutoSize = true;
            this.buttonChangeFontBuiltin2.Location = new System.Drawing.Point(183, 117);
            this.buttonChangeFontBuiltin2.Name = "buttonChangeFontBuiltin2";
            this.buttonChangeFontBuiltin2.Size = new System.Drawing.Size(23, 23);
            this.buttonChangeFontBuiltin2.TabIndex = 13;
            this.buttonChangeFontBuiltin2.Text = "2";
            this.buttonChangeFontBuiltin2.UseVisualStyleBackColor = true;
            this.buttonChangeFontBuiltin2.Click += new System.EventHandler(this.buttonChangeFontBuiltin2_Click);
            // 
            // ConfigurationForm
            // 
            this.AcceptButton = this.buttonSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(324, 481);
            this.Controls.Add(this.buttonChangeFontBuiltin2);
            this.Controls.Add(this.buttonChangeFontBuiltin1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.pictureBoxFontPreview);
            this.Controls.Add(this.buttonChangeFont);
            this.Controls.Add(this.labelFont);
            this.Controls.Add(this.textBoxFontOverlapXY);
            this.Controls.Add(this.labelFontOverlapXY);
            this.Controls.Add(this.textBoxTargetLoadMS);
            this.Controls.Add(this.labelTargetLoadMS);
            this.Controls.Add(this.textBoxTargetChunksMemory);
            this.Controls.Add(this.labelTargetChunksMemory);
            this.Controls.Add(this.checkBoxForceGC);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "ConfigurationForm";
            this.Text = "Change TtyRecMonkey Configuration";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxFontPreview)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.CheckBox checkBoxForceGC;
		private System.Windows.Forms.Label labelTargetChunksMemory;
		private System.Windows.Forms.TextBox textBoxTargetChunksMemory;
		private System.Windows.Forms.Label labelTargetLoadMS;
		private System.Windows.Forms.TextBox textBoxTargetLoadMS;
		private System.Windows.Forms.Label labelFontOverlapXY;
		private System.Windows.Forms.TextBox textBoxFontOverlapXY;
		private System.Windows.Forms.Label labelFont;
		private System.Windows.Forms.Button buttonChangeFont;
		private System.Windows.Forms.PictureBox pictureBoxFontPreview;
		private System.Windows.Forms.Button buttonSave;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonChangeFontBuiltin1;
		private System.Windows.Forms.Button buttonChangeFontBuiltin2;

	}
}