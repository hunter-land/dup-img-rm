namespace ImageCompare
{
	partial class Picker
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

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.RightImage = new System.Windows.Forms.PictureBox();
			this.LeftImage = new System.Windows.Forms.PictureBox();
			this.label1 = new System.Windows.Forms.Label();
			this.ButtonMatch = new System.Windows.Forms.Button();
			this.ButtonDiffer = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.LeftInfo = new System.Windows.Forms.Label();
			this.RightInfo = new System.Windows.Forms.Label();
			this.CenterInfo = new System.Windows.Forms.Label();
			this.ProgressBarActive = new System.Windows.Forms.ProgressBar();
			this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
			this.fileSystemWatcher1 = new System.IO.FileSystemWatcher();
			this.ProgressBarPassive = new System.Windows.Forms.ProgressBar();
			((System.ComponentModel.ISupportInitialize)(this.RightImage)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.LeftImage)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcher1)).BeginInit();
			this.SuspendLayout();
			// 
			// RightImage
			// 
			this.RightImage.AccessibleName = "";
			this.RightImage.Location = new System.Drawing.Point(403, 24);
			this.RightImage.Name = "RightImage";
			this.RightImage.Size = new System.Drawing.Size(385, 359);
			this.RightImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.RightImage.TabIndex = 0;
			this.RightImage.TabStop = false;
			// 
			// LeftImage
			// 
			this.LeftImage.Location = new System.Drawing.Point(12, 24);
			this.LeftImage.Name = "LeftImage";
			this.LeftImage.Size = new System.Drawing.Size(385, 359);
			this.LeftImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.LeftImage.TabIndex = 1;
			this.LeftImage.TabStop = false;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(403, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(385, 12);
			this.label1.TabIndex = 2;
			this.label1.Text = "Right Image";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ButtonMatch
			// 
			this.ButtonMatch.Location = new System.Drawing.Point(298, 418);
			this.ButtonMatch.Name = "ButtonMatch";
			this.ButtonMatch.Size = new System.Drawing.Size(99, 23);
			this.ButtonMatch.TabIndex = 3;
			this.ButtonMatch.Text = "Pictures Match";
			this.ButtonMatch.UseVisualStyleBackColor = true;
			this.ButtonMatch.Click += new System.EventHandler(this.ButtonMatch_Click);
			// 
			// ButtonDiffer
			// 
			this.ButtonDiffer.Location = new System.Drawing.Point(403, 418);
			this.ButtonDiffer.Name = "ButtonDiffer";
			this.ButtonDiffer.Size = new System.Drawing.Size(99, 23);
			this.ButtonDiffer.TabIndex = 4;
			this.ButtonDiffer.Text = "Pictures Differ";
			this.ButtonDiffer.UseVisualStyleBackColor = true;
			this.ButtonDiffer.Click += new System.EventHandler(this.ButtonDiffer_Click);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(12, 9);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(385, 12);
			this.label2.TabIndex = 5;
			this.label2.Text = "Left Image";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// LeftInfo
			// 
			this.LeftInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.LeftInfo.Location = new System.Drawing.Point(12, 418);
			this.LeftInfo.Name = "LeftInfo";
			this.LeftInfo.Size = new System.Drawing.Size(280, 45);
			this.LeftInfo.TabIndex = 6;
			this.LeftInfo.Text = "Info about the left image";
			this.LeftInfo.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// RightInfo
			// 
			this.RightInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.RightInfo.Location = new System.Drawing.Point(508, 418);
			this.RightInfo.Name = "RightInfo";
			this.RightInfo.Size = new System.Drawing.Size(280, 45);
			this.RightInfo.TabIndex = 7;
			this.RightInfo.Text = "Info about the right image";
			this.RightInfo.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// CenterInfo
			// 
			this.CenterInfo.BackColor = System.Drawing.Color.Transparent;
			this.CenterInfo.Location = new System.Drawing.Point(298, 444);
			this.CenterInfo.Name = "CenterInfo";
			this.CenterInfo.Size = new System.Drawing.Size(204, 19);
			this.CenterInfo.TabIndex = 8;
			this.CenterInfo.Text = "Center Info";
			this.CenterInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ProgressBarActive
			// 
			this.ProgressBarActive.Location = new System.Drawing.Point(15, 418);
			this.ProgressBarActive.Name = "ProgressBarActive";
			this.ProgressBarActive.Size = new System.Drawing.Size(776, 45);
			this.ProgressBarActive.TabIndex = 9;
			this.ProgressBarActive.Visible = false;
			// 
			// fileSystemWatcher1
			// 
			this.fileSystemWatcher1.EnableRaisingEvents = true;
			this.fileSystemWatcher1.SynchronizingObject = this;
			// 
			// ProgressBarPassive
			// 
			this.ProgressBarPassive.Location = new System.Drawing.Point(15, 389);
			this.ProgressBarPassive.Name = "ProgressBarPassive";
			this.ProgressBarPassive.Size = new System.Drawing.Size(773, 23);
			this.ProgressBarPassive.TabIndex = 10;
			// 
			// Picker
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 472);
			this.Controls.Add(this.ProgressBarPassive);
			this.Controls.Add(this.CenterInfo);
			this.Controls.Add(this.ProgressBarActive);
			this.Controls.Add(this.RightInfo);
			this.Controls.Add(this.LeftInfo);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.ButtonDiffer);
			this.Controls.Add(this.ButtonMatch);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.LeftImage);
			this.Controls.Add(this.RightImage);
			this.Name = "Picker";
			this.Text = "Form1";
			((System.ComponentModel.ISupportInitialize)(this.RightImage)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.LeftImage)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcher1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.PictureBox RightImage;
		private System.Windows.Forms.PictureBox LeftImage;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button ButtonDiffer;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label LeftInfo;
		private System.Windows.Forms.Label RightInfo;
		private System.Windows.Forms.Label CenterInfo;
		private System.Windows.Forms.Button ButtonMatch;
		private System.Windows.Forms.ProgressBar ProgressBarActive;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
		private System.IO.FileSystemWatcher fileSystemWatcher1;
		private System.Windows.Forms.ProgressBar ProgressBarPassive;
	}
}

