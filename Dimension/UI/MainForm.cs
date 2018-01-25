using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dimension
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            Text = "Dimension Private Alpha #" + Program.buildNumber.ToString();
            setColors();
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        public void selectUser(Model.Peer z)
        {
            UI.UserPanel b = new UI.UserPanel(z);
            b.Dock = DockStyle.Fill;
            Program.mainForm.addOrSelectPanel(z.username, b, "(!) Files for " + z.id.ToString());

        }
        public void privateChatReceived(Model.Commands.PrivateChatCommand c, Model.Peer z)
        {
            UI.UserPanel b = new UI.UserPanel(z);
            b.Dock = DockStyle.Fill;

            this.Invoke(new Action(delegate ()
            {
                Control p = Program.mainForm.addOrSelectPanel(z.username, b, "(!) Files for " + z.id.ToString());

                ((UI.UserPanel)p).selectChat();
                ((UI.UserPanel)p).addLine(DateTime.Now.ToShortTimeString() + " " + z.username + ": " + c.content);
                flash();
            }));
        }
        public void flash()
        {
            try
            {
                this.Invoke(new Action(delegate ()
                {
                    try
                    {
                        if (!UI.FlashWindow.ApplicationIsActivated())
                        {
                                if (Program.settings.getBool("Flash on Name Drop", true))
                                    UI.FlashWindow.Flash(this);
                                if (Program.settings.getBool("Play sounds", true))
                                {
                                    System.Media.SoundPlayer p = new System.Media.SoundPlayer(Properties.Resources.Bell);
                                    p.Play();
                                }
                            }
                    }
                    catch
                    {

                    }
                }));
            }
            catch
            {

            }
        }
        public static bool isMono
        {
            get
            {
                return Type.GetType("Mono.Runtime") != null;
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            var t = new UI.TransfersPanel();
            t.Dock = DockStyle.Fill;
            splitContainer1.Panel2.Controls.Add(t);
            

            setColors();

            windowToolStrip.Items.Clear();
            
            UI.HTMLPanel hp = new UI.HTMLPanel();
            hp.Dock = DockStyle.Fill;
            addOrSelectPanel("Welcome", hp, "Welcome");
            
        }
        void deselect()
        {
            if (currentPanel is UI.SelectableTab)
                ((UI.SelectableTab)currentPanel).unselect();

            foreach (ToolStripItem z in windowToolStrip.Items)
                ((ToolStripButton)z).Checked = false;

        }
        void select(ToolStripButton b)
        {
            deselect();
            b.Checked = true;
            contentPanel.Controls.Clear();
            contentPanel.Controls.Add((Control)b.Tag);
            currentPanel = (Control) b.Tag;
            if (currentPanel is UI.SelectableTab)
                ((UI.SelectableTab)currentPanel).select();

        }
        Control currentPanel = null;
        public Control addOrSelectPanel(string text, Control panel, string tag)
        {
            foreach (ToolStripItem z in windowToolStrip.Items)
            {
                if ((string)((Control)z.Tag).Tag == tag)
                {
                    select((ToolStripButton)z);
                    return (Control)z.Tag;
                }
            }
            deselect();
            ToolStripButton b = new ToolStripButton();
            b.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            b.Text = text;
            b.Click += panelButtonClicked;
            b.MouseDown += panelButtonMouseDown;
            b.Checked = true;
            b.Tag = panel;
            contentPanel.Controls.Clear();
            panel.Tag = tag;
            contentPanel.Controls.Add(panel);
            currentPanel = panel;
            windowToolStrip.Items.Add(b);
            if (currentPanel is UI.SelectableTab)
                ((UI.SelectableTab)currentPanel).select();
            return panel;
        }
        ToolStripButton rightClicked;

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (rightClicked != null)
            {
                closeTab(rightClicked);
            }
        }
        void closeTab(ToolStripButton b)
        {
            if (b.Checked)
                contentPanel.Controls.Clear();
            if (b.Tag is Model.ClosableTab)
                ((Model.ClosableTab)b.Tag).close();
            if (b.Tag is Control)
                ((Control)b.Tag).Dispose();
            windowToolStrip.Items.Remove(b);
            windowToolStrip.Refresh();
        }
        void panelButtonMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                rightClicked = (ToolStripButton)sender;
                contextMenuStrip.Show(Cursor.Position);
            }
            if (e.Button == MouseButtons.Middle)
            {
                closeTab((ToolStripButton)sender);
            }
        }
        void panelButtonClicked(object sender, EventArgs e)
        {
            deselect();
            ((ToolStripButton)sender).Checked = true;
            select((ToolStripButton)sender);


        }
        
        private void joinLANButton_Click(object sender, EventArgs e)
        {
            joinLANCircle();
        }
        private void joinLANCircleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            joinLANCircle();
        }
        void joinLANCircle()
        {
            UI.CirclePanel c = new UI.CirclePanel();
            c.Dock = DockStyle.Fill;
            addOrSelectPanel("LAN", c, "LAN");
        }

        TabPage clickedPage;
        

        private void joinInternetCircle_Click(object sender, EventArgs e)
        {
            doJoinInternetCircle();
        }
        private void joinInternetCircleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            doJoinInternetCircle();
        }
        void doJoinInternetCircle()
        {
            UI.JoinCircleForm j = new UI.JoinCircleForm(UI.JoinCircleForm.CircleType.bootstrap);
            j.ShowDialog();
        }
        public void addInternetCircle(System.Net.IPEndPoint[] endpoints, string url, UI.JoinCircleForm.CircleType circleType)
        {
            for(int i=0; i<windowToolStrip.Items.Count; i++)
                if(windowToolStrip.Items[i].Tag is UI.CirclePanel)
                    if (((UI.CirclePanel)windowToolStrip.Items[i].Tag).url.ToLower() == url.ToLower())
                        {
                            select((ToolStripButton)windowToolStrip.Items[i]);
                            return;
                        }
            TabPage p = new TabPage("Internet Circle");
            if (url.StartsWith("#"))
                p.Text = url;
            else
            {
                string s = url;
                if (s.Contains("//"))
                    s = s.Substring(s.IndexOf("//") + 2);
                if (s.Contains("/"))
                    s = s.Substring(0, s.IndexOf("/"));
                p.Text = s;
            }
            UI.CirclePanel c = new UI.CirclePanel(url, circleType);
            c.Dock = DockStyle.Fill;

            addOrSelectPanel(p.Text, c, url);
        }
        void showDownloadQueue()
        {
            UI.DownloadQueuePanel c = new UI.DownloadQueuePanel();
            c.Dock = DockStyle.Fill;
            addOrSelectPanel("Download Queue", c, "Download Queue");
        }
        private void downloadQueueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showDownloadQueue();
        }

        private void downloadQueueButton_Click(object sender, EventArgs e)
        {
            showDownloadQueue();
        }
        void showSettings()
        {
            UI.SettingsForm f = new UI.SettingsForm();
            f.ShowDialog();
        }
        private void settingsButton_Click(object sender, EventArgs e)
        {
            showSettings();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showSettings();
        }

        void showCompletedDownloads()
        {
            UI.FinishedTransfersPanel c = new UI.FinishedTransfersPanel();
            c.Dock = DockStyle.Fill;
            addOrSelectPanel("Completed Downloads", c, "Completed Downloads");
        }
        private void completedDownloadsButton_Click(object sender, EventArgs e)
        {
            showCompletedDownloads();
        }
        void showCompletedUploads()
        {
            UI.FinishedTransfersPanel c = new UI.FinishedTransfersPanel();
            c.Dock = DockStyle.Fill;
            addOrSelectPanel("Completed Uploads", c, "Completed Uploads");

        }

        private void finishedDownloadsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showCompletedDownloads();
        }

        private void completedUploadsButton_Click(object sender, EventArgs e)
        {
            showCompletedUploads();
        }

        private void finishedUploadsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showCompletedUploads();
        }

        void showSearch()
        {
            UI.SearchPanel c = new UI.SearchPanel();
            c.Dock = DockStyle.Fill;
            addOrSelectPanel("Search", c, "Search");
        }

        private void searchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showSearch();
        }

        private void searchButton_Click(object sender, EventArgs e)
        {
            showSearch();
        }
        void showHashProgress()
        {
            UI.HashProgressForm f = new UI.HashProgressForm();
            f.ShowDialog();
        }

        private void hashingProgressToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showHashProgress();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UI.AboutForm f = new UI.AboutForm();
            f.ShowDialog();
        }

        //TODO: Fix all Close All whatever options
        private void closeAllCirclesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*for (int i = 0; i < tabControl.TabPages.Count; i++)
            {
                if (tabControl.TabPages[i].Controls[0] is UI.CirclePanel)
                {
                    //TODO: Actually leave the circles
                    tabControl.TabPages.RemoveAt(i);
                    i--;
                }
                }*/
        }
        
        private void closeAllFileListsToolStripMenuItem_Click(object sender, EventArgs e)
        {

            /*
            for (int i = 0; i < tabControl.TabPages.Count; i++)
            {
                if (tabControl.TabPages[i].Controls[0] is UI.UserPanel)
                {
                    tabControl.TabPages.RemoveAt(i);
                    i--;
                }
            }*/
        }

        private void closeAllSearchesToolStripMenuItem_Click(object sender, EventArgs e)
        {

            /*
            for (int i = 0; i < tabControl.TabPages.Count; i++)
            {
                if (tabControl.TabPages[i].Controls[0] is UI.SearchPanel)
                {
                    tabControl.TabPages.RemoveAt(i);
                    i--;
                }
            }*/
        }

        private void dHTTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void updateLogTimer_Tick(object sender, EventArgs e)
        {
            if (logStatus.Text != Model.SystemLog.lastLine)
                logStatus.Text = Model.SystemLog.lastLine;
            if (Program.kademlia.ready)
                kadReadyLabel.Visible = true;

            Model.Transfer[] z;
            lock (Model.Transfer.transfers)
                z = Model.Transfer.transfers.ToArray();
            if (z.Length == 0 && splitContainer1.Panel2Collapsed == false)
                splitContainer1.Panel2Collapsed = true;
            if (z.Length > 0 && splitContainer1.Panel2Collapsed == true)
                splitContainer1.Panel2Collapsed = false;

        }

        private void joinKademliaCircleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UI.JoinCircleForm j = new UI.JoinCircleForm(UI.JoinCircleForm.CircleType.kademlia);
            j.ShowDialog();
        }

        private void transferRateTimer_Tick(object sender, EventArgs e)
        {
            speedLabel.Text = UI.ByteFormatter.formatBytes(Program.globalUpCounter.frontBuffer) + "/s up; " +
                UI.ByteFormatter.formatBytes(Program.globalDownCounter.frontBuffer) + "/s down. Total " +
                 UI.ByteFormatter.formatBytes(Program.globalUpCounter.totalBytes) + " up; " +
                UI.ByteFormatter.formatBytes(Program.globalDownCounter.totalBytes) + " down.";
        }
        

        private void networkStatusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UI.NetworkStatusPanel s = new UI.NetworkStatusPanel();
            s.Dock = DockStyle.Fill;

            addOrSelectPanel("Network Status", s, "Network Status");
        }
        void openDownloadFolder()
        {
            string s = Program.settings.getString("Default Download Folder", "C:\\Downloads");
            if (System.IO.Directory.Exists(s))
                System.Diagnostics.Process.Start(s);
        }
        private void openDownloadsFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openDownloadFolder();
        }

        private void openDownloadsFolderButton_Click(object sender, EventArgs e)
        {
            openDownloadFolder();
        }
        

        private void limitButton_Click(object sender, EventArgs e)
        {
            if (Program.settings.getULong("Global Upload Rate Limit", 0) == 0)
                uploadSpeedToolStripMenuItem.Text = "Upload Speed: None";
            else
                uploadSpeedToolStripMenuItem.Text = "Upload Speed: " + UI.ByteFormatter.formatBytes(Program.settings.getULong("Global Upload Rate Limit", 0))+"/s";
            if (Program.settings.getULong("Global Download Rate Limit", 0) == 0)
                downloadSpeedToolStripMenuItem.Text = "Download Speed: None";
            else
                downloadSpeedToolStripMenuItem.Text = "Download Speed: " + UI.ByteFormatter.formatBytes(Program.settings.getULong("Global Download Rate Limit", 0)) + "/s";

        }

        private void downloadSpeedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UI.LimitChangeDialog d = new UI.LimitChangeDialog(UI.LimitChangeDialog.WhichLimit.down);
            d.ShowDialog();
        }

        private void uploadSpeedToolStripMenuItem_Click(object sender, EventArgs e)
        {

            UI.LimitChangeDialog d = new UI.LimitChangeDialog(UI.LimitChangeDialog.WhichLimit.up);
            d.ShowDialog();
        }

        private void invertedColorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Program.settings.setBool("Invert Colors", !invertedColorsToolStripMenuItem.Checked);
            setColors();
        }
        public void setColors()
        {
            invertedColorsToolStripMenuItem.Checked = Program.settings.getBool("Invert Colors", false);
            colorChange?.Invoke(Program.settings.getBool("Invert Colors", false));
        }
        public delegate void ColorChangeEvent(bool inverted = false);
        public static event ColorChangeEvent colorChange;
        

        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (Program.settings.getBool("Auto Rejoin on Startup", true))
            {
                if (Program.settings.getBool("Joined LAN Circle", false))
                    joinLANCircle();
                foreach (string s in Program.settings.getStringArray("Bootstrap Circles Open"))
                    UI.JoinCircleForm.joinCircle(s, UI.JoinCircleForm.CircleType.bootstrap);
                foreach (string s in Program.settings.getStringArray("Kademlia Circles Open"))
                    UI.JoinCircleForm.joinCircle(s, UI.JoinCircleForm.CircleType.kademlia);
            }
        }

    }
}