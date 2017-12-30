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
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            var p = new UI.SystemLogPanel();
            p.Dock = DockStyle.Fill;
            systemLogStartingPage.Controls.Add(p);

            var t = new UI.TransfersPanel();
            t.Dock = DockStyle.Fill;
            splitContainer1.Panel2.Controls.Add(t);
        }

        private void joinLANButton_Click(object sender, EventArgs e)
        {
            joinLANCircle();
        }
        private void joinLANCircleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            joinLANCircle();
        }
        public void createOrSelect(TabPage p)
        {
            for (int i = 0; i < tabControl.TabPages.Count; i++)
                if ((string)tabControl.TabPages[i].Tag == (string)p.Tag)
                {
                    tabControl.SelectTab(i);
                    return;
                }
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
                tabControl.TabPages.Remove(clickedPage);
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
            UI.JoinCircleForm j = new UI.JoinCircleForm();
            j.ShowDialog();
        }
        public void addInternetCircle(System.Net.IPEndPoint[] endpoints, string url)
        {
            for (int i = 0; i < tabControl.TabPages.Count; i++)
                if (tabControl.TabPages[i].Controls[0] is UI.CirclePanel)
                    if (((UI.CirclePanel)tabControl.TabPages[i].Controls[0]).url == url)
                    {
                        tabControl.SelectTab(i);
                        return;
                    }
            TabPage p = new TabPage("Internet Circle");
            UI.CirclePanel c = new UI.CirclePanel(url);
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

        private void closeAllPrivateMessagesToolStripMenuItem_Click(object sender, EventArgs e)
        {

            for (int i = 0; i < tabControl.TabPages.Count; i++)
            {
                if (tabControl.TabPages[i].Controls[0] is UI.PrivateMessagePanel)
                {
                    tabControl.TabPages.RemoveAt(i);
                    i--;
                }
            }
        }

        private void closeAllOfflinePrivateMessagesToolStripMenuItem_Click(object sender, EventArgs e)
        {

            for (int i = 0; i < tabControl.TabPages.Count; i++)
            {
                if (tabControl.TabPages[i].Controls[0] is UI.PrivateMessagePanel)
                {
                    tabControl.TabPages.RemoveAt(i);
                    i--;
                }
            }

        }

        private void closeAllFileListsToolStripMenuItem_Click(object sender, EventArgs e)
        {


            for (int i = 0; i < tabControl.TabPages.Count; i++)
            {
                if (tabControl.TabPages[i].Controls[0] is UI.FileBrowserPanel)
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
        }
    }
}