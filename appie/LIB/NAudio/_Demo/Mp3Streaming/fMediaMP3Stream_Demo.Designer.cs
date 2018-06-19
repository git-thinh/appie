namespace appie.FORM
{
    partial class fMediaMP3Stream_Demo
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
            this.mp3StreamingPanel1 = new NAudio.Mp3StreamingPanel();
            this.SuspendLayout();
            // 
            // mp3StreamingPanel1
            // 
            this.mp3StreamingPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mp3StreamingPanel1.Location = new System.Drawing.Point(0, 0);
            this.mp3StreamingPanel1.Name = "mp3StreamingPanel1";
            this.mp3StreamingPanel1.Size = new System.Drawing.Size(524, 158);
            this.mp3StreamingPanel1.TabIndex = 0;
            // 
            // fMediaMP3Stream_Demo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(524, 158);
            this.Controls.Add(this.mp3StreamingPanel1);
            this.Name = "fMediaMP3Stream_Demo";
            this.Text = "fMediaMP3Stream_Demo";
            this.ResumeLayout(false);

        }

        #endregion

        private NAudio.Mp3StreamingPanel mp3StreamingPanel1;
    }
}