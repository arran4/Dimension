namespace Dimension.UI
{
    partial class SettingsForm
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
            this.saveButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.fontSelectBox = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.browseDownloadsButton = new System.Windows.Forms.Button();
            this.downloadFolderInput = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.descriptionBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.usernameBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.dhtPortBox = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.useUDTBox = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.udpControlPortBox = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.udpDataPortBox = new System.Windows.Forms.NumericUpDown();
            this.manuallyForwardPortsButton = new System.Windows.Forms.RadioButton();
            this.UPnPButton = new System.Windows.Forms.RadioButton();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.deleteShareButton = new System.Windows.Forms.Button();
            this.addShareButton = new System.Windows.Forms.Button();
            this.sharesListView = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.flashNameDropBox = new System.Windows.Forms.CheckBox();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dhtPortBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udpControlPortBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udpDataPortBox)).BeginInit();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(404, 306);
            this.saveButton.Margin = new System.Windows.Forms.Padding(2);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(56, 19);
            this.saveButton.TabIndex = 0;
            this.saveButton.TabStop = false;
            this.saveButton.Text = "Okay";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(9, 306);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(2);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(56, 19);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(9, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(451, 289);
            this.tabControl1.TabIndex = 2;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.flashNameDropBox);
            this.tabPage1.Controls.Add(this.fontSelectBox);
            this.tabPage1.Controls.Add(this.label9);
            this.tabPage1.Controls.Add(this.browseDownloadsButton);
            this.tabPage1.Controls.Add(this.downloadFolderInput);
            this.tabPage1.Controls.Add(this.label7);
            this.tabPage1.Controls.Add(this.descriptionBox);
            this.tabPage1.Controls.Add(this.label4);
            this.tabPage1.Controls.Add(this.usernameBox);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(443, 263);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "User";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // fontSelectBox
            // 
            this.fontSelectBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.fontSelectBox.FormattingEnabled = true;
            this.fontSelectBox.Items.AddRange(new object[] {
            "Courier New",
            "Lucida Console",
            "Consolas"});
            this.fontSelectBox.Location = new System.Drawing.Point(98, 79);
            this.fontSelectBox.Name = "fontSelectBox";
            this.fontSelectBox.Size = new System.Drawing.Size(337, 21);
            this.fontSelectBox.TabIndex = 8;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(65, 82);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(31, 13);
            this.label9.TabIndex = 7;
            this.label9.Text = "Font:";
            // 
            // browseDownloadsButton
            // 
            this.browseDownloadsButton.Location = new System.Drawing.Point(359, 53);
            this.browseDownloadsButton.Name = "browseDownloadsButton";
            this.browseDownloadsButton.Size = new System.Drawing.Size(75, 23);
            this.browseDownloadsButton.TabIndex = 6;
            this.browseDownloadsButton.Text = "Browse";
            this.browseDownloadsButton.UseVisualStyleBackColor = true;
            this.browseDownloadsButton.Click += new System.EventHandler(this.browseDownloadsButton_Click);
            // 
            // downloadFolderInput
            // 
            this.downloadFolderInput.Location = new System.Drawing.Point(98, 53);
            this.downloadFolderInput.Name = "downloadFolderInput";
            this.downloadFolderInput.Size = new System.Drawing.Size(255, 20);
            this.downloadFolderInput.TabIndex = 5;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 53);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(90, 13);
            this.label7.TabIndex = 4;
            this.label7.Text = "Download Folder:";
            // 
            // descriptionBox
            // 
            this.descriptionBox.Location = new System.Drawing.Point(98, 30);
            this.descriptionBox.MaxLength = 255;
            this.descriptionBox.Name = "descriptionBox";
            this.descriptionBox.Size = new System.Drawing.Size(339, 20);
            this.descriptionBox.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(33, 33);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Description:";
            // 
            // usernameBox
            // 
            this.usernameBox.Location = new System.Drawing.Point(98, 5);
            this.usernameBox.MaxLength = 16;
            this.usernameBox.Name = "usernameBox";
            this.usernameBox.Size = new System.Drawing.Size(337, 20);
            this.usernameBox.TabIndex = 1;
            this.usernameBox.TextChanged += new System.EventHandler(this.usernameBox_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(38, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Username:";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.dhtPortBox);
            this.tabPage2.Controls.Add(this.label8);
            this.tabPage2.Controls.Add(this.useUDTBox);
            this.tabPage2.Controls.Add(this.label6);
            this.tabPage2.Controls.Add(this.label5);
            this.tabPage2.Controls.Add(this.label3);
            this.tabPage2.Controls.Add(this.udpControlPortBox);
            this.tabPage2.Controls.Add(this.label2);
            this.tabPage2.Controls.Add(this.udpDataPortBox);
            this.tabPage2.Controls.Add(this.manuallyForwardPortsButton);
            this.tabPage2.Controls.Add(this.UPnPButton);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(443, 263);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Network";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // dhtPortBox
            // 
            this.dhtPortBox.Location = new System.Drawing.Point(117, 116);
            this.dhtPortBox.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
            this.dhtPortBox.Name = "dhtPortBox";
            this.dhtPortBox.Size = new System.Drawing.Size(69, 20);
            this.dhtPortBox.TabIndex = 10;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(30, 118);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(81, 13);
            this.label8.TabIndex = 9;
            this.label8.Text = "UDP DHT Port:";
            // 
            // useUDTBox
            // 
            this.useUDTBox.AutoSize = true;
            this.useUDTBox.Location = new System.Drawing.Point(17, 155);
            this.useUDTBox.Name = "useUDTBox";
            this.useUDTBox.Size = new System.Drawing.Size(71, 17);
            this.useUDTBox.TabIndex = 8;
            this.useUDTBox.Text = "Use UDT";
            this.useUDTBox.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(14, 238);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(317, 13);
            this.label6.TabIndex = 7;
            this.label6.Text = "Dimension must be restarted for port/UPnP settings to take effect.";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(191, 67);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(56, 12);
            this.label5.TabIndex = 6;
            this.label5.Text = "(0 = random)";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(22, 92);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(91, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "UDP Control Port:";
            // 
            // udpControlPortBox
            // 
            this.udpControlPortBox.Location = new System.Drawing.Point(116, 90);
            this.udpControlPortBox.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
            this.udpControlPortBox.Name = "udpControlPortBox";
            this.udpControlPortBox.Size = new System.Drawing.Size(69, 20);
            this.udpControlPortBox.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(32, 66);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "TCP Data Port:";
            // 
            // udpDataPortBox
            // 
            this.udpDataPortBox.Location = new System.Drawing.Point(116, 64);
            this.udpDataPortBox.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
            this.udpDataPortBox.Name = "udpDataPortBox";
            this.udpDataPortBox.Size = new System.Drawing.Size(69, 20);
            this.udpDataPortBox.TabIndex = 2;
            // 
            // manuallyForwardPortsButton
            // 
            this.manuallyForwardPortsButton.AutoSize = true;
            this.manuallyForwardPortsButton.Location = new System.Drawing.Point(17, 41);
            this.manuallyForwardPortsButton.Name = "manuallyForwardPortsButton";
            this.manuallyForwardPortsButton.Size = new System.Drawing.Size(135, 17);
            this.manuallyForwardPortsButton.TabIndex = 1;
            this.manuallyForwardPortsButton.TabStop = true;
            this.manuallyForwardPortsButton.Text = "Manually Forward Ports";
            this.manuallyForwardPortsButton.UseVisualStyleBackColor = true;
            // 
            // UPnPButton
            // 
            this.UPnPButton.AutoSize = true;
            this.UPnPButton.Checked = true;
            this.UPnPButton.Location = new System.Drawing.Point(17, 18);
            this.UPnPButton.Name = "UPnPButton";
            this.UPnPButton.Size = new System.Drawing.Size(75, 17);
            this.UPnPButton.TabIndex = 0;
            this.UPnPButton.TabStop = true;
            this.UPnPButton.Text = "Use UPnP";
            this.UPnPButton.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.deleteShareButton);
            this.tabPage3.Controls.Add(this.addShareButton);
            this.tabPage3.Controls.Add(this.sharesListView);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(443, 263);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Shares";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // deleteShareButton
            // 
            this.deleteShareButton.Location = new System.Drawing.Point(84, 240);
            this.deleteShareButton.Name = "deleteShareButton";
            this.deleteShareButton.Size = new System.Drawing.Size(75, 23);
            this.deleteShareButton.TabIndex = 2;
            this.deleteShareButton.Text = "Delete";
            this.deleteShareButton.UseVisualStyleBackColor = true;
            this.deleteShareButton.Click += new System.EventHandler(this.deleteShareButton_Click);
            // 
            // addShareButton
            // 
            this.addShareButton.Location = new System.Drawing.Point(3, 240);
            this.addShareButton.Name = "addShareButton";
            this.addShareButton.Size = new System.Drawing.Size(75, 23);
            this.addShareButton.TabIndex = 1;
            this.addShareButton.Text = "Add Share";
            this.addShareButton.UseVisualStyleBackColor = true;
            this.addShareButton.Click += new System.EventHandler(this.addShareButton_Click);
            // 
            // sharesListView
            // 
            this.sharesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.sharesListView.FullRowSelect = true;
            this.sharesListView.Location = new System.Drawing.Point(3, 3);
            this.sharesListView.Name = "sharesListView";
            this.sharesListView.Size = new System.Drawing.Size(437, 234);
            this.sharesListView.TabIndex = 0;
            this.sharesListView.UseCompatibleStateImageBehavior = false;
            this.sharesListView.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Path";
            this.columnHeader2.Width = 206;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Size";
            this.columnHeader3.Width = 76;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Hashed";
            // 
            // flashNameDropBox
            // 
            this.flashNameDropBox.AutoSize = true;
            this.flashNameDropBox.Location = new System.Drawing.Point(98, 106);
            this.flashNameDropBox.Name = "flashNameDropBox";
            this.flashNameDropBox.Size = new System.Drawing.Size(188, 17);
            this.flashNameDropBox.TabIndex = 9;
            this.flashNameDropBox.Text = "Flash Taskbar Item on Name Drop";
            this.flashNameDropBox.UseVisualStyleBackColor = true;
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(467, 334);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.saveButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dhtPortBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udpControlPortBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udpDataPortBox)).EndInit();
            this.tabPage3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TextBox usernameBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown udpControlPortBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown udpDataPortBox;
        private System.Windows.Forms.RadioButton manuallyForwardPortsButton;
        private System.Windows.Forms.RadioButton UPnPButton;
        private System.Windows.Forms.ListView sharesListView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Button addShareButton;
        private System.Windows.Forms.TextBox descriptionBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button browseDownloadsButton;
        private System.Windows.Forms.TextBox downloadFolderInput;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.Button deleteShareButton;
        private System.Windows.Forms.CheckBox useUDTBox;
        private System.Windows.Forms.NumericUpDown dhtPortBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox fontSelectBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox flashNameDropBox;
    }
}