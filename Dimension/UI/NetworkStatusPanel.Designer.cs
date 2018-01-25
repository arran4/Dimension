namespace Dimension.UI
{
    partial class NetworkStatusPanel
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
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.portsTextBox = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.eventsBox = new System.Windows.Forms.TextBox();
            this.updateTimer = new System.Windows.Forms.Timer(this.components);
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.trafficBox = new System.Windows.Forms.TextBox();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.systemLogBox = new System.Windows.Forms.TextBox();
            this.tabControl.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPage1);
            this.tabControl.Controls.Add(this.tabPage2);
            this.tabControl.Controls.Add(this.tabPage3);
            this.tabControl.Controls.Add(this.tabPage4);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(775, 419);
            this.tabControl.TabIndex = 1;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.portsTextBox);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(767, 393);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Ports";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // portsTextBox
            // 
            this.portsTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.portsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.portsTextBox.Location = new System.Drawing.Point(3, 3);
            this.portsTextBox.Multiline = true;
            this.portsTextBox.Name = "portsTextBox";
            this.portsTextBox.ReadOnly = true;
            this.portsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.portsTextBox.Size = new System.Drawing.Size(761, 387);
            this.portsTextBox.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.eventsBox);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(767, 393);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Events";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // eventsBox
            // 
            this.eventsBox.BackColor = System.Drawing.SystemColors.Window;
            this.eventsBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.eventsBox.Location = new System.Drawing.Point(3, 3);
            this.eventsBox.Multiline = true;
            this.eventsBox.Name = "eventsBox";
            this.eventsBox.ReadOnly = true;
            this.eventsBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.eventsBox.Size = new System.Drawing.Size(761, 387);
            this.eventsBox.TabIndex = 1;
            // 
            // updateTimer
            // 
            this.updateTimer.Enabled = true;
            this.updateTimer.Interval = 500;
            this.updateTimer.Tick += new System.EventHandler(this.updateTimer_Tick);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.trafficBox);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(767, 393);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Traffic";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // trafficBox
            // 
            this.trafficBox.BackColor = System.Drawing.SystemColors.Window;
            this.trafficBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.trafficBox.Location = new System.Drawing.Point(0, 0);
            this.trafficBox.Multiline = true;
            this.trafficBox.Name = "trafficBox";
            this.trafficBox.ReadOnly = true;
            this.trafficBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.trafficBox.Size = new System.Drawing.Size(767, 393);
            this.trafficBox.TabIndex = 1;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.systemLogBox);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(767, 393);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "System Log";
            // 
            // systemLogBox
            // 
            this.systemLogBox.BackColor = System.Drawing.SystemColors.Window;
            this.systemLogBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.systemLogBox.Location = new System.Drawing.Point(0, 0);
            this.systemLogBox.Multiline = true;
            this.systemLogBox.Name = "systemLogBox";
            this.systemLogBox.ReadOnly = true;
            this.systemLogBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.systemLogBox.Size = new System.Drawing.Size(767, 393);
            this.systemLogBox.TabIndex = 1;
            // 
            // NetworkStatusPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl);
            this.Name = "NetworkStatusPanel";
            this.Size = new System.Drawing.Size(775, 419);
            this.tabControl.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TextBox portsTextBox;
        private System.Windows.Forms.TextBox eventsBox;
        private System.Windows.Forms.Timer updateTimer;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TextBox trafficBox;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TextBox systemLogBox;
    }
}
