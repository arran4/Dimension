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
            TabPage p = new TabPage();
            p.Text = z.username;
            p.Tag = "(!) Files for " + z.id.ToString();
            UI.UserPanel b = new UI.UserPanel(z);
            b.Dock = DockStyle.Fill;
            p.Controls.Add(b);
            Program.mainForm.createOrSelect(p, true);

        }
        public void privateChatReceived(Model.Commands.PrivateChatCommand c, Model.Peer z)
        {
            TabPage p = new TabPage();
            p.Text = z.username;
            p.Tag = "(!) Files for " + z.id.ToString();
            UI.UserPanel b = new UI.UserPanel(z);
            b.Dock = DockStyle.Fill;
            p.Controls.Add(b);
            this.Invoke(new Action(delegate ()
            {
                Program.mainForm.createOrSelect(p, true);
                ((UI.UserPanel)p.Controls[0]).selectChat();
                ((UI.UserPanel)p.Controls[0]).addLine(DateTime.Now.ToShortTimeString() + " " + z.username + ": " + c.content);
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

            tabControl.TabPages.Clear();
            TabPage h = new TabPage("Welcome");
            h.Tag = h.Text;
            UI.HTMLPanel hp = new UI.HTMLPanel();
            hp.Dock = DockStyle.Fill;
            h.Controls.Add(hp);
            tabControl.TabPages.Add(h);
            
        }

        private void joinLANButton_Click(object sender, EventArgs e)
        {
            joinLANCircle();
        }
        private void joinLANCircleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            joinLANCircle();
        }
        public void createOrSelect(TabPage p, bool highlight = false)
        {
            for (int i = 0; i < tabControl.TabPages.Count; i++)
                if ((string)tabControl.TabPages[i].Tag == (string)p.Tag)
                {
                    if (highlight && !tabControl.TabPages[i].Text.StartsWith("(!)") && i != tabControl.SelectedIndex)
                        tabControl.TabPages[i].Text = "(!) " + tabControl.TabPages[i].Text;

                    if (!highlight)
                        tabControl.SelectTab(i);
                    return;
                }
            if (highlight && !p.Text.StartsWith("(!)"))
                p.Text = "(!) " + p.Text;
            tabControl.TabPages.Add(p);
            tabControl.SelectTab(tabControl.TabPages.Count - 1);

        }
        void joinLANCircle()
        {
            TabPage p = new TabPage("LAN");
            UI.CirclePanel c = new UI.CirclePanel();
            c.Dock = DockStyle.Fill;
            p.Controls.Add(c);
            p.Tag = "LAN Circle";
            createOrSelect(p);
        }

        TabPage clickedPage;
        private void tabControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                clickedPage = null;
                for (int i = 0; i < tabControl.TabPages.Count; i++)
                    if (tabControl.GetTabRect(i).Contains(tabControl.PointToClient(MousePosition)))
                        clickedPage = tabControl.TabPages[i];
                if (clickedPage != null)
                {
                    //TODO: if it's a circle, actually leave it
                    if(clickedPage.Controls[0] is Model.ClosableTab)
                        ((Model.ClosableTab)clickedPage.Controls[0]).close();
                    tabControl.TabPages.Remove(clickedPage);
                }
            }
            if (e.Button == MouseButtons.Right)
            {
                clickedPage = null;
                for (int i = 0; i < tabControl.TabPages.Count; i++)
                    if (tabControl.GetTabRect(i).Contains(tabControl.PointToClient(MousePosition)))
                        clickedPage = tabControl.TabPages[i];
                contextMenuStrip.Show(Cursor.Position);
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Todo: if it's a circle, actually leave it
            if (clickedPage != null)
            {
                if (clickedPage.Controls[0] is Model.ClosableTab)
                    ((Model.ClosableTab)clickedPage.Controls[0]).close();
                tabControl.TabPages.Remove(clickedPage);
            }
        }

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
            for (int i = 0; i < tabControl.TabPages.Count; i++)
                if(tabControl.TabPages[i].Controls.Count > 0)
                    if (tabControl.TabPages[i].Controls[0] is UI.CirclePanel)
                        if (((UI.CirclePanel)tabControl.TabPages[i].Controls[0]).url.ToLower() == url.ToLower())
                        {
                            tabControl.SelectTab(i);
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
            p.Controls.Add(c);
            p.Tag = "Internet Circle";
            tabControl.TabPages.Add(p);
            tabControl.SelectTab(tabControl.TabPages.Count - 1);
        }
        void showDownloadQueue()
        {
            TabPage p = new TabPage("Download Queue");
            UI.DownloadQueuePanel c = new UI.DownloadQueuePanel();
            c.Dock = DockStyle.Fill;
            p.Controls.Add(c);
            p.Tag = "Download Queue";
            createOrSelect(p);
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
            TabPage p = new TabPage("Completed Downloads");
            UI.FinishedTransfersPanel c = new UI.FinishedTransfersPanel();
            c.Dock = DockStyle.Fill;
            p.Controls.Add(c);
            p.Tag = "Completed Downloads";
            createOrSelect(p);
        }
        private void completedDownloadsButton_Click(object sender, EventArgs e)
        {
            showCompletedDownloads();
        }
        void showCompletedUploads()
        {
            TabPage p = new TabPage("Completed Uploads");
            UI.FinishedTransfersPanel c = new UI.FinishedTransfersPanel();
            c.Dock = DockStyle.Fill;
            p.Controls.Add(c);
            p.Tag = "Completed Uploads";
            createOrSelect(p);
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
            TabPage p = new TabPage("Search");
            UI.SearchPanel c = new UI.SearchPanel();
            c.Dock = DockStyle.Fill;
            p.Controls.Add(c);
            p.Tag = "Search";
            tabControl.TabPages.Add(p);
            tabControl.SelectTab(tabControl.TabPages.Count - 1);
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

        private void closeAllCirclesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < tabControl.TabPages.Count; i++)
            {
                if (tabControl.TabPages[i].Controls[0] is UI.CirclePanel)
                {
                    //TODO: Actually leave the circles
                    tabControl.TabPages.RemoveAt(i);
                    i--;
                }
                }
        }
        
        private void closeAllFileListsToolStripMenuItem_Click(object sender, EventArgs e)
        {


            for (int i = 0; i < tabControl.TabPages.Count; i++)
            {
                if (tabControl.TabPages[i].Controls[0] is UI.UserPanel)
                {
                    tabControl.TabPages.RemoveAt(i);
                    i--;
                }
            }
        }

        private void closeAllSearchesToolStripMenuItem_Click(object sender, EventArgs e)
        {


            for (int i = 0; i < tabControl.TabPages.Count; i++)
            {
                if (tabControl.TabPages[i].Controls[0] is UI.SearchPanel)
                {
                    tabControl.TabPages.RemoveAt(i);
                    i--;
                }
            }
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

        private void systemLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UI.SystemLogPanel s = new UI.SystemLogPanel();
            TabPage p = new TabPage("System Log");
            s.Dock = DockStyle.Fill;
            p.Controls.Add(s);
            p.Tag = "System Log";

            createOrSelect(p);
        }

        private void networkStatusToolStripMenuItem_Click(object sender, EventArgs e)
        {

            UI.NetworkStatusPanel s = new UI.NetworkStatusPanel();
            TabPage p = new TabPage("Network Status");
            s.Dock = DockStyle.Fill;
            p.Controls.Add(s);
            p.Tag = "Network Status";

            createOrSelect(p);
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

        private void tabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabPage tp = tabControl.TabPages[e.Index];

            bool highlight = tp.Text.StartsWith("(!) ");

            using (SolidBrush brush =
                   new SolidBrush(!highlight ? tp.BackColor : SystemColors.Highlight))
            using (SolidBrush textBrush =
                   new SolidBrush(!highlight ? tp.ForeColor : SystemColors.HighlightText))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
                e.Graphics.DrawString(tp.Text, e.Font, textBrush, e.Bounds.X + 2, e.Bounds.Y + 4);
            }
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

        TabPage lastSelection = null;
        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab != null)
            {
                if (tabControl.SelectedTab.Text.StartsWith("(!)"))
                    tabControl.SelectedTab.Text = tabControl.SelectedTab.Text.Substring(3);
                if (tabControl.SelectedTab.Controls.Count > 0)
                    if (tabControl.SelectedTab.Controls[0] is UI.SelectableTab)
                        ((UI.SelectableTab)tabControl.SelectedTab.Controls[0]).select();
                if(lastSelection != null)
                    if (lastSelection.Controls[0] is UI.SelectableTab)
                        ((UI.SelectableTab)lastSelection.Controls[0]).unselect();
                lastSelection = tabControl.SelectedTab;
            }
        }

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