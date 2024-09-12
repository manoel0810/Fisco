namespace FiscoCoreTeste
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            View = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)View).BeginInit();
            SuspendLayout();
            // 
            // View
            // 
            View.BorderStyle = BorderStyle.FixedSingle;
            View.Location = new Point(20, 35);
            View.Name = "View";
            View.Size = new Size(461, 654);
            View.TabIndex = 0;
            View.TabStop = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(501, 725);
            Controls.Add(View);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Views";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)View).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox View;
    }
}
