namespace Dimension
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.logStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.speedLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.joinLANCircleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.joinInternetCircleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.joinKademliaCircleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.openDownloadsFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.downloadQueueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.finishedUploadsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.finishedDownloadsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.searchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hashingProgressToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.windowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeAllCirclesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeAllPrivateMessagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeAllOfflinePrivateMessagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeAllFileListsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeAllSearchesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.joinLANButton = new System.Windows.Forms.ToolStripButton();
            this.joinInternetCircle = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.settingsButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.openDownloadsFolderButton = new System.Windows.Forms.ToolStripButton();
            this.downloadQueueButton = new System.Windows.Forms.ToolStripButton();
            this.completedDownloadsButton = new System.Windows.Forms.ToolStripButton();
            this.completedUploadsButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.searchButton = new System.Windows.Forms.ToolStripButton();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.systemLogStartingPage = new System.Windows.Forms.TabPage();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateLogTimer = new System.Windows.Forms.Timer(this.components);
            this.kadReadyLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip
            // 
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.logStatus,
            this.kadReadyLabel,
            this.speedLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 379);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 10, 0);
            this.statusStrip.Size = new System.Drawing.Size(724, 22);
            this.statusStrip.TabIndex = 0;
            this.statusStrip.Text = "statusStrip1";
            // 
            // logStatus
            // 
            this.logStatus.Name = "logStatus";
            this.logStatus.Size = new System.Drawing.Size(190, 17);
            this.logStatus.Text = "Successfully started up Dimension.";
            // 
            // speedLabel
            // 
            this.speedLabel.Name = "speedLabel";
            this.speedLabel.Size = new System.Drawing.Size(398, 17);
            this.speedLabel.Spring = true;
            this.speedLabel.Text = "0B/s up; 0B/s down. Total 0B up, 0B down.";
            this.speedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.windowToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(4, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(724, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.menuStrip1_ItemClicked);
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.joinLANCircleToolStripMenuItem,
            this.joinInternetCircleToolStripMenuItem,
            this.joinKademliaCircleToolStripMenuItem,
            this.toolStripMenuItem1,
            this.openDownloadsFolderToolStripMenuItem,
            this.toolStripMenuItem2,
            this.settingsToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            this.fileToolStripMenuItem.Click += new System.EventHandler(this.fileToolStripMenuItem_Click);
            // 
            // joinLANCircleToolStripMenuItem
            // 
            this.joinLANCircleToolStripMenuItem.Name = "joinLANCircleToolStripMenuItem";
            this.joinLANCircleToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.joinLANCircleToolStripMenuItem.Text = "Join LAN Circle";
            this.joinLANCircleToolStripMenuItem.Click += new System.EventHandler(this.joinLANCircleToolStripMenuItem_Click);
            // 
            // joinInternetCircleToolStripMenuItem
            // 
            this.joinInternetCircleToolStripMenuItem.Name = "joinInternetCircleToolStripMenuItem";
            this.joinInternetCircleToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.joinInternetCircleToolStripMenuItem.Text = "Join Bootstrap Circle...";
            this.joinInternetCircleToolStripMenuItem.Click += new System.EventHandler(this.joinInternetCircleToolStripMenuItem_Click);
            // 
            // joinKademliaCircleToolStripMenuItem
            // 
            this.joinKademliaCircleToolStripMenuItem.Name = "joinKademliaCircleToolStripMenuItem";
            this.joinKademliaCircleToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.joinKademliaCircleToolStripMenuItem.Text = "Join Kademlia Circle...";
            this.joinKademliaCircleToolStripMenuItem.Click += new System.EventHandler(this.joinKademliaCircleToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(207, 6);
            // 
            // openDownloadsFolderToolStripMenuItem
            // 
            this.openDownloadsFolderToolStripMenuItem.Name = "openDownloadsFolderToolStripMenuItem";
            this.openDownloadsFolderToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.openDownloadsFolderToolStripMenuItem.Text = "Open Downloads Folder...";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(207, 6);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.settingsToolStripMenuItem.Text = "Settings...";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.downloadQueueToolStripMenuItem,
            this.finishedUploadsToolStripMenuItem,
            this.finishedDownloadsToolStripMenuItem,
            this.toolStripMenuItem3,
            this.searchToolStripMenuItem,
            this.hashingProgressToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // downloadQueueToolStripMenuItem
            // 
            this.downloadQueueToolStripMenuItem.Name = "downloadQueueToolStripMenuItem";
            this.downloadQueueToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.downloadQueueToolStripMenuItem.Text = "Download Queue";
            this.downloadQueueToolStripMenuItem.Click += new System.EventHandler(this.downloadQueueToolStripMenuItem_Click);
            // 
            // finishedUploadsToolStripMenuItem
            // 
            this.finishedUploadsToolStripMenuItem.Name = "finishedUploadsToolStripMenuItem";
            this.finishedUploadsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.finishedUploadsToolStripMenuItem.Text = "Finished Uploads";
            this.finishedUploadsToolStripMenuItem.Click += new System.EventHandler(this.finishedUploadsToolStripMenuItem_Click);
            // 
            // finishedDownloadsToolStripMenuItem
            // 
            this.finishedDownloadsToolStripMenuItem.Name = "finishedDownloadsToolStripMenuItem";
            this.finishedDownloadsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.finishedDownloadsToolStripMenuItem.Text = "Finished Downloads";
            this.finishedDownloadsToolStripMenuItem.Click += new System.EventHandler(this.finishedDownloadsToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(177, 6);
            // 
            // searchToolStripMenuItem
            // 
            this.searchToolStripMenuItem.Name = "searchToolStripMenuItem";
            this.searchToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.searchToolStripMenuItem.Text = "Search";
            this.searchToolStripMenuItem.Click += new System.EventHandler(this.searchToolStripMenuItem_Click);
            // 
            // hashingProgressToolStripMenuItem
            // 
            this.hashingProgressToolStripMenuItem.Name = "hashingProgressToolStripMenuItem";
            this.hashingProgressToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.hashingProgressToolStripMenuItem.Text = "Hashing Progress...";
            this.hashingProgressToolStripMenuItem.Click += new System.EventHandler(this.hashingProgressToolStripMenuItem_Click);
            // 
            // windowToolStripMenuItem
            // 
            this.windowToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.closeAllCirclesToolStripMenuItem,
            this.closeAllPrivateMessagesToolStripMenuItem,
            this.closeAllOfflinePrivateMessagesToolStripMenuItem,
            this.closeAllFileListsToolStripMenuItem,
            this.closeAllSearchesToolStripMenuItem});
            this.windowToolStripMenuItem.Name = "windowToolStripMenuItem";
            this.windowToolStripMenuItem.Size = new System.Drawing.Size(63, 20);
            this.windowToolStripMenuItem.Text = "Window";
            // 
            // closeAllCirclesToolStripMenuItem
            // 
            this.closeAllCirclesToolStripMenuItem.Name = "closeAllCirclesToolStripMenuItem";
            this.closeAllCirclesToolStripMenuItem.Size = new System.Drawing.Size(252, 22);
            this.closeAllCirclesToolStripMenuItem.Text = "Close All Circles";
            this.closeAllCirclesToolStripMenuItem.Click += new System.EventHandler(this.closeAllCirclesToolStripMenuItem_Click);
            // 
            // closeAllPrivateMessagesToolStripMenuItem
            // 
            this.closeAllPrivateMessagesToolStripMenuItem.Name = "closeAllPrivateMessagesToolStripMenuItem";
            this.closeAllPrivateMessagesToolStripMenuItem.Size = new System.Drawing.Size(252, 22);
            this.closeAllPrivateMessagesToolStripMenuItem.Text = "Close All Private Messages";
            this.closeAllPrivateMessagesToolStripMenuItem.Click += new System.EventHandler(this.closeAllPrivateMessagesToolStripMenuItem_Click);
            // 
            // closeAllOfflinePrivateMessagesToolStripMenuItem
            // 
            this.closeAllOfflinePrivateMessagesToolStripMenuItem.Name = "closeAllOfflinePrivateMessagesToolStripMenuItem";
            this.closeAllOfflinePrivateMessagesToolStripMenuItem.Size = new System.Drawing.Size(252, 22);
            this.closeAllOfflinePrivateMessagesToolStripMenuItem.Text = "Close All Offline Private Messages";
            this.closeAllOfflinePrivateMessagesToolStripMenuItem.Click += new System.EventHandler(this.closeAllOfflinePrivateMessagesToolStripMenuItem_Click);
            // 
            // closeAllFileListsToolStripMenuItem
            // 
            this.closeAllFileListsToolStripMenuItem.Name = "closeAllFileListsToolStripMenuItem";
            this.closeAllFileListsToolStripMenuItem.Size = new System.Drawing.Size(252, 22);
            this.closeAllFileListsToolStripMenuItem.Text = "Close All File Lists";
            this.closeAllFileListsToolStripMenuItem.Click += new System.EventHandler(this.closeAllFileListsToolStripMenuItem_Click);
            // 
            // closeAllSearchesToolStripMenuItem
            // 
            this.closeAllSearchesToolStripMenuItem.Name = "closeAllSearchesToolStripMenuItem";
            this.closeAllSearchesToolStripMenuItem.Size = new System.Drawing.Size(252, 22);
            this.closeAllSearchesToolStripMenuItem.Text = "Close All Searches";
            this.closeAllSearchesToolStripMenuItem.Click += new System.EventHandler(this.closeAllSearchesToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.aboutToolStripMenuItem.Text = "About...";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // toolStrip
            // 
            this.toolStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.joinLANButton,
            this.joinInternetCircle,
            this.toolStripSeparator1,
            this.settingsButton,
            this.toolStripSeparator2,
            this.openDownloadsFolderButton,
            this.downloadQueueButton,
            this.completedDownloadsButton,
            this.completedUploadsButton,
            this.toolStripSeparator3,
            this.searchButton});
            this.toolStrip.Location = new System.Drawing.Point(0, 24);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(724, 27);
            this.toolStrip.TabIndex = 2;
            this.toolStrip.Text = "toolStrip1";
            // 
            // joinLANButton
            // 
            this.joinLANButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.joinLANButton.Image = ((System.Drawing.Image)(resources.GetObject("joinLANButton.Image")));
            this.joinLANButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.joinLANButton.Name = "joinLANButton";
            this.joinLANButton.Size = new System.Drawing.Size(24, 24);
            this.joinLANButton.Text = "Join LAN Circle";
            this.joinLANButton.Click += new System.EventHandler(this.joinLANButton_Click);
            // 
            // joinInternetCircle
            // 
            this.joinInternetCircle.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.joinInternetCircle.Image = ((System.Drawing.Image)(resources.GetObject("joinInternetCircle.Image")));
            this.joinInternetCircle.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.joinInternetCircle.Name = "joinInternetCircle";
            this.joinInternetCircle.Size = new System.Drawing.Size(24, 24);
            this.joinInternetCircle.Text = "Join Internet Circle";
            this.joinInternetCircle.Click += new System.EventHandler(this.joinInternetCircle_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 27);
            // 
            // settingsButton
            // 
            this.settingsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.settingsButton.Image = ((System.Drawing.Image)(resources.GetObject("settingsButton.Image")));
            this.settingsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.settingsButton.Name = "settingsButton";
            this.settingsButton.Size = new System.Drawing.Size(24, 24);
            this.settingsButton.Text = "Settings";
            this.settingsButton.Click += new System.EventHandler(this.settingsButton_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 27);
            // 
            // openDownloadsFolderButton
            // 
            this.openDownloadsFolderButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.openDownloadsFolderButton.Image = ((System.Drawing.Image)(resources.GetObject("openDownloadsFolderButton.Image")));
            this.openDownloadsFolderButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openDownloadsFolderButton.Name = "openDownloadsFolderButton";
            this.openDownloadsFolderButton.Size = new System.Drawing.Size(24, 24);
            this.openDownloadsFolderButton.Text = "Open Downloads Folder";
            // 
            // downloadQueueButton
            // 
            this.downloadQueueButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.downloadQueueButton.Image = ((System.Drawing.Image)(resources.GetObject("downloadQueueButton.Image")));
            this.downloadQueueButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.downloadQueueButton.Name = "downloadQueueButton";
            this.downloadQueueButton.Size = new System.Drawing.Size(24, 24);
            this.downloadQueueButton.Text = "Download Queue";
            this.downloadQueueButton.Click += new System.EventHandler(this.downloadQueueButton_Click);
            // 
            // completedDownloadsButton
            // 
            this.completedDownloadsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.completedDownloadsButton.Image = ((System.Drawing.Image)(resources.GetObject("completedDownloadsButton.Image")));
            this.completedDownloadsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.completedDownloadsButton.Name = "completedDownloadsButton";
            this.completedDownloadsButton.Size = new System.Drawing.Size(24, 24);
            this.completedDownloadsButton.Text = "Completed Downloads";
            this.completedDownloadsButton.Click += new System.EventHandler(this.completedDownloadsButton_Click);
            // 
            // completedUploadsButton
            // 
            this.completedUploadsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.completedUploadsButton.Image = ((System.Drawing.Image)(resources.GetObject("completedUploadsButton.Image")));
            this.completedUploadsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.completedUploadsButton.Name = "completedUploadsButton";
            this.completedUploadsButton.Size = new System.Drawing.Size(24, 24);
            this.completedUploadsButton.Text = "Completed Uploads";
            this.completedUploadsButton.Click += new System.EventHandler(this.completedUploadsButton_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 27);
            // 
            // searchButton
            // 
            this.searchButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.searchButton.Image = ((System.Drawing.Image)(resources.GetObject("searchButton.Image")));
            this.searchButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.searchButton.Name = "searchButton";
            this.searchButton.Size = new System.Drawing.Size(24, 24);
            this.searchButton.Text = "Search";
            this.searchButton.Click += new System.EventHandler(this.searchButton_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 51);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tabControl);
            this.splitContainer1.Size = new System.Drawing.Size(724, 328);
            this.splitContainer1.SplitterDistance = 193;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.TabIndex = 3;
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.systemLogStartingPage);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(724, 193);
            this.tabControl.TabIndex = 0;
            this.tabControl.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tabControl_MouseDown);
            // 
            // systemLogStartingPage
            // 
            this.systemLogStartingPage.Location = new System.Drawing.Point(4, 22);
            this.systemLogStartingPage.Margin = new System.Windows.Forms.Padding(2);
            this.systemLogStartingPage.Name = "systemLogStartingPage";
            this.systemLogStartingPage.Padding = new System.Windows.Forms.Padding(2);
            this.systemLogStartingPage.Size = new System.Drawing.Size(716, 167);
            this.systemLogStartingPage.TabIndex = 0;
            this.systemLogStartingPage.Text = "System Log";
            this.systemLogStartingPage.UseVisualStyleBackColor = true;
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.closeToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(104, 26);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.closeToolStripMenuItem.Text = "Close";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // updateLogTimer
            // 
            this.updateLogTimer.Enabled = true;
            this.updateLogTimer.Tick += new System.EventHandler(this.updateLogTimer_Tick);
            // 
            // kadReadyLabel
            // 
            this.kadReadyLabel.Name = "kadReadyLabel";
            this.kadReadyLabel.Size = new System.Drawing.Size(94, 17);
            this.kadReadyLabel.Text = "Kademlia Ready.";
            this.kadReadyLabel.Visible = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(724, 401);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(483, 330);
            this.Name = "MainForm";
            this.Text = "Dimension";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem joinLANCircleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem joinInternetCircleToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem openDownloadsFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem downloadQueueToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem finishedDownloadsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem searchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hashingProgressToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem windowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeAllCirclesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeAllPrivateMessagesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeAllOfflinePrivateMessagesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeAllFileListsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeAllSearchesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton joinLANButton;
        private System.Windows.Forms.ToolStripButton joinInternetCircle;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton settingsButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton openDownloadsFolderButton;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage systemLogStartingPage;
        private System.Windows.Forms.ToolStripButton downloadQueueButton;
        private System.Windows.Forms.ToolStripButton completedDownloadsButton;
        private System.Windows.Forms.ToolStripButton completedUploadsButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton searchButton;
        private System.Windows.Forms.ToolStripStatusLabel logStatus;
        private System.Windows.Forms.ToolStripStatusLabel speedLabel;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem finishedUploadsToolStripMenuItem;
        private System.Windows.Forms.Timer updateLogTimer;
        private System.Windows.Forms.ToolStripMenuItem joinKademliaCircleToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel kadReadyLabel;
    }
}

