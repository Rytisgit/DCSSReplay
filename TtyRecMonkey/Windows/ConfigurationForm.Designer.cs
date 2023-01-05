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
            this.labelTargetChunksMemory = new System.Windows.Forms.Label();
            this.labelTargetLoadMS = new System.Windows.Forms.Label();
            this.labelFontOverlapXY = new System.Windows.Forms.Label();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown3 = new System.Windows.Forms.NumericUpDown();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.ClassicVersionRadioButton = new System.Windows.Forms.RadioButton();
            this.Version2023RadioButton = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelTargetChunksMemory
            // 
            this.labelTargetChunksMemory.AutoSize = true;
            this.labelTargetChunksMemory.Location = new System.Drawing.Point(12, 9);
            this.labelTargetChunksMemory.Name = "labelTargetChunksMemory";
            this.labelTargetChunksMemory.Size = new System.Drawing.Size(140, 13);
            this.labelTargetChunksMemory.TabIndex = 5;
            this.labelTargetChunksMemory.Text = "Pause between frames (ms):";
            this.labelTargetChunksMemory.Click += new System.EventHandler(this.labelTargetChunksMemory_Click);
            // 
            // labelTargetLoadMS
            // 
            this.labelTargetLoadMS.AutoSize = true;
            this.labelTargetLoadMS.Location = new System.Drawing.Point(12, 34);
            this.labelTargetLoadMS.Name = "labelTargetLoadMS";
            this.labelTargetLoadMS.Size = new System.Drawing.Size(139, 13);
            this.labelTargetLoadMS.TabIndex = 7;
            this.labelTargetLoadMS.Text = "Time Jump Arrow Keys (ms):";
            // 
            // labelFontOverlapXY
            // 
            this.labelFontOverlapXY.AutoSize = true;
            this.labelFontOverlapXY.Location = new System.Drawing.Point(12, 60);
            this.labelFontOverlapXY.Name = "labelFontOverlapXY";
            this.labelFontOverlapXY.Size = new System.Drawing.Size(183, 13);
            this.labelFontOverlapXY.TabIndex = 9;
            this.labelFontOverlapXY.Text = "Maximum Wait Between Frames (ms):";
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(10, 213);
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
            this.buttonCancel.Location = new System.Drawing.Point(257, 213);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 16;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(212, 7);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown1.TabIndex = 17;
            // 
            // numericUpDown2
            // 
            this.numericUpDown2.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDown2.Location = new System.Drawing.Point(212, 34);
            this.numericUpDown2.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numericUpDown2.Name = "numericUpDown2";
            this.numericUpDown2.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown2.TabIndex = 18;
            // 
            // numericUpDown3
            // 
            this.numericUpDown3.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDown3.Location = new System.Drawing.Point(212, 60);
            this.numericUpDown3.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numericUpDown3.Name = "numericUpDown3";
            this.numericUpDown3.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown3.TabIndex = 19;
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(13, 35);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(51, 17);
            this.radioButton1.TabIndex = 20;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "None";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(111, 35);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(74, 17);
            this.radioButton2.TabIndex = 21;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "File Select";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // radioButton3
            // 
            this.radioButton3.AutoSize = true;
            this.radioButton3.Location = new System.Drawing.Point(202, 35);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(106, 17);
            this.radioButton3.TabIndex = 22;
            this.radioButton3.TabStop = true;
            this.radioButton3.Text = "Ttyrec Download";
            this.radioButton3.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(133, 13);
            this.label1.TabIndex = 23;
            this.label1.Text = "Window To Open On Start";
            // 
            // ClassicVersionRadioButton
            // 
            this.ClassicVersionRadioButton.AutoSize = true;
            this.ClassicVersionRadioButton.Location = new System.Drawing.Point(13, 29);
            this.ClassicVersionRadioButton.Name = "ClassicVersionRadioButton";
            this.ClassicVersionRadioButton.Size = new System.Drawing.Size(58, 17);
            this.ClassicVersionRadioButton.TabIndex = 24;
            this.ClassicVersionRadioButton.TabStop = true;
            this.ClassicVersionRadioButton.Text = "Classic";
            this.ClassicVersionRadioButton.UseVisualStyleBackColor = true;
            // 
            // Version2023RadioButton
            // 
            this.Version2023RadioButton.AutoSize = true;
            this.Version2023RadioButton.Location = new System.Drawing.Point(111, 29);
            this.Version2023RadioButton.Name = "Version2023RadioButton";
            this.Version2023RadioButton.Size = new System.Drawing.Size(49, 17);
            this.Version2023RadioButton.TabIndex = 25;
            this.Version2023RadioButton.TabStop = true;
            this.Version2023RadioButton.Text = "2023";
            this.Version2023RadioButton.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(2, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(125, 13);
            this.label2.TabIndex = 27;
            this.label2.Text = "Default Tile Data Version";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButton3);
            this.groupBox1.Controls.Add(this.radioButton2);
            this.groupBox1.Controls.Add(this.radioButton1);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(10, 86);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(322, 58);
            this.groupBox1.TabIndex = 28;
            this.groupBox1.TabStop = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.Version2023RadioButton);
            this.groupBox2.Controls.Add(this.ClassicVersionRadioButton);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Location = new System.Drawing.Point(10, 144);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(322, 58);
            this.groupBox2.TabIndex = 29;
            this.groupBox2.TabStop = false;
            // 
            // ConfigurationForm
            // 
            this.AcceptButton = this.buttonSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(346, 248);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.numericUpDown3);
            this.Controls.Add(this.numericUpDown2);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.labelFontOverlapXY);
            this.Controls.Add(this.labelTargetLoadMS);
            this.Controls.Add(this.labelTargetChunksMemory);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "ConfigurationForm";
            this.Text = "Change TtyRecMonkey Configuration";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label labelTargetChunksMemory;
		private System.Windows.Forms.Label labelTargetLoadMS;
		private System.Windows.Forms.Label labelFontOverlapXY;
		private System.Windows.Forms.Button buttonSave;
		private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.NumericUpDown numericUpDown2;
        private System.Windows.Forms.NumericUpDown numericUpDown3;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton ClassicVersionRadioButton;
        private System.Windows.Forms.RadioButton Version2023RadioButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
    }
}