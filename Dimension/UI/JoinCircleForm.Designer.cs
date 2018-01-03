namespace Dimension.UI
{
    partial class JoinCircleForm
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
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.urlBox = new System.Windows.Forms.TextBox();
            this.joinButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.kadInitLabel = new System.Windows.Forms.Label();
            this.kadInitLabelTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 7);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Address:";
            // 
            // urlBox
            // 
            this.urlBox.Location = new System.Drawing.Point(70, 7);
            this.urlBox.Margin = new System.Windows.Forms.Padding(2);
            this.urlBox.Name = "urlBox";
            this.urlBox.Size = new System.Drawing.Size(290, 20);
            this.urlBox.TabIndex = 1;
            this.urlBox.Text = "http://www.9thcircle.net/Projects/Dimension/bootstrap.php";
            this.urlBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.urlBox_KeyDown);
            // 
            // joinButton
            // 
            this.joinButton.Location = new System.Drawing.Point(303, 41);
            this.joinButton.Margin = new System.Windows.Forms.Padding(2);
            this.joinButton.Name = "joinButton";
            this.joinButton.Size = new System.Drawing.Size(56, 19);
            this.joinButton.TabIndex = 2;
            this.joinButton.Text = "Join";
            this.joinButton.UseVisualStyleBackColor = true;
            this.joinButton.Click += new System.EventHandler(this.joinButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(11, 41);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(2);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(56, 19);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // kadInitLabel
            // 
            this.kadInitLabel.AutoSize = true;
            this.kadInitLabel.Location = new System.Drawing.Point(43, 26);
            this.kadInitLabel.Name = "kadInitLabel";
            this.kadInitLabel.Size = new System.Drawing.Size(285, 13);
            this.kadInitLabel.TabIndex = 4;
            this.kadInitLabel.Text = "Kademlia is initializing, this might take a couple of minutes...";
            this.kadInitLabel.Visible = false;
            // 
            // kadInitLabelTimer
            // 
            this.kadInitLabelTimer.Enabled = true;
            this.kadInitLabelTimer.Tick += new System.EventHandler(this.kadInitLabelTimer_Tick);
            // 
            // JoinCircleForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(368, 70);
            this.Controls.Add(this.kadInitLabel);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.joinButton);
            this.Controls.Add(this.urlBox);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "JoinCircleForm";
            this.Text = "Join Internet Circle";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox urlBox;
        private System.Windows.Forms.Button joinButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label kadInitLabel;
        private System.Windows.Forms.Timer kadInitLabelTimer;
    }
}