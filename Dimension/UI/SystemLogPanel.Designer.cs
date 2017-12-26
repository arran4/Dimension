namespace Dimension.UI
{
    partial class SystemLogPanel
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.contentBox = new System.Windows.Forms.TextBox();
            this.updateTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // contentBox
            // 
            this.contentBox.BackColor = System.Drawing.SystemColors.Window;
            this.contentBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.contentBox.Location = new System.Drawing.Point(0, 0);
            this.contentBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.contentBox.Multiline = true;
            this.contentBox.Name = "contentBox";
            this.contentBox.ReadOnly = true;
            this.contentBox.Size = new System.Drawing.Size(460, 362);
            this.contentBox.TabIndex = 0;
            this.contentBox.Text = "System log here.";
            // 
            // updateTimer
            // 
            this.updateTimer.Enabled = true;
            this.updateTimer.Interval = 250;
            this.updateTimer.Tick += new System.EventHandler(this.updateTimer_Tick);
            // 
            // SystemLogPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.contentBox);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "SystemLogPanel";
            this.Size = new System.Drawing.Size(460, 362);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox contentBox;
        private System.Windows.Forms.Timer updateTimer;
    }
}
