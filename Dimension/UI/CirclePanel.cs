using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dimension.UI
{
    public partial class CirclePanel : UserControl
    {
        List<string> haveAdded = new List<string>();
        void joinLoop()
        {
            while (true)
            {
                System.Net.IPEndPoint[] e = null;

                if (circleType == JoinCircleForm.CircleType.bootstrap)
                {
                    e = Program.bootstrap.join(url);

                }
                else if (circleType == JoinCircleForm.CircleType.kademlia)
                {
                    Program.kademlia.announce(url.ToLower().Trim());
                    System.Threading.Thread.Sleep(10000);
                    e = Program.kademlia.doLookup(url.ToLower().Trim());
                }
                foreach (var z in e)
                {
                    if (z != null)
                    {
                        string s = z.Address.ToString() + ":" + z.Port;
                        if (!haveAdded.Contains(s))
                        {
                            haveAdded.Add(s);
                            Program.theCore.addPeer(z);

                        }
                    }
                }

                System.Threading.Thread.Sleep(10000);
            }
        }
        public string url;
        UI.JoinCircleForm.CircleType circleType;
        public CirclePanel(string url, UI.JoinCircleForm.CircleType circleType)
        {
            this.circleType = circleType;
            this.url = url;
            InitializeComponent();
            setupUserList();

            updateFont();

            System.Security.Cryptography.SHA512Managed sha = new System.Security.Cryptography.SHA512Managed();

            circleHash = BitConverter.ToUInt64(sha.ComputeHash(Encoding.UTF8.GetBytes(url.ToLower())), 0);
            Program.theCore.joinCircle(url);
            Program.theCore.chatReceivedEvent += chatReceived;
            MainForm.colorChange += colorChanged;
            Program.mainForm.setColors();


            System.Threading.Thread t = new System.Threading.Thread(joinLoop);
            t.IsBackground = true;
            t.Name = "Circle join loop";
            t.Start();
        }
        public CirclePanel()
        {
            InitializeComponent();
            setupUserList();

            url = "LAN";
            Program.theCore.joinCircle(url);
            System.Security.Cryptography.SHA512Managed sha = new System.Security.Cryptography.SHA512Managed();

            circleHash = BitConverter.ToUInt64(sha.ComputeHash(Encoding.UTF8.GetBytes(url.ToLower())), 0);
            Program.theCore.chatReceivedEvent += chatReceived;
            MainForm.colorChange += colorChanged;
            Program.mainForm.setColors();
        }
        ulong circleHash;
        Model.Peer[] allPeersInCircle
        {
            get
            {
                return Program.theCore.peerManager.allPeersInCircle(circleHash);
            }
        }
        void doUpdateUserList()
        {
            var items = allPeersInCircle;

            userListView.BeginUpdate();
            while (userListView.Items.Count < items.Length)
            {
                ListViewItem i = new ListViewItem();
                i.SubItems.Add("");
                i.SubItems.Add("");
                userListView.Items.Add(i);
            }
            while (userListView.Items.Count > items.Length)
                userListView.Items.RemoveAt(0);
            for (int i = 0; i < items.Length; i++)
            {
                userListView.Items[i].Tag = items[i];

                /* string s1 = items[i].username;
                 string s2 = "";

                 for (int i2 = 0; i2 < s1.Length; i2++)
                     if (char.IsLetterOrDigit(s1[i2]) || s1[i2] == ' ' || s1[i2] == '_')
                         s2 += s1[i2];*/

                string s2 = items[i].username;

                if (items[i].maybeDead)
                    userListView.Items[i].ForeColor = SystemColors.GrayText;
                if (items[i].probablyDead)
                    userListView.Items[i].Font = new Font(userListView.Font, FontStyle.Italic);
                else
                    userListView.Items[i].Font = new Font(userListView.Font, FontStyle.Regular);

                if (items[i].afk.HasValue)
                    userListView.Items[i].Text = s2 + (items[i].afk.Value ? " (AFK)" : "");
                else
                    userListView.Items[i].Text = s2;
                userListView.Items[i].SubItems[1].Text = (items[i].buildNumber.ToString());
                userListView.Items[i].SubItems[2].Text = (ByteFormatter.formatBytes(items[i].share));
            }
            userListView.EndUpdate();
        }
        string lastFingerprint = "";
        void updateUserList(Model.Peer p, ulong channel)
        {
            updateUserList(p);

        }
        void updateUserList(Model.Peer p)
        {
            var items = allPeersInCircle;
            if (items.Length == 0)
                return;
            string fingerprint = "";
            foreach (Model.Peer p2 in items)
                if (!p2.quit)
                    fingerprint += p2.username + p2.share.ToString() + p2.buildNumber.ToString() + p2.afk.ToString();
            if (lastFingerprint == fingerprint)
                return;
            lastFingerprint = fingerprint;
            if (this.InvokeRequired)
            {
                try
                {
                    Invoke(new Action(delegate ()
                    {
                        doUpdateUserList();
                    }));
                }
                catch (InvalidOperationException)
                {
                }
            }
            else
            {
                doUpdateUserList();
            }
        }
        void colorChanged(bool invert)
        {
            if (invert)
            {
                historyBox.BackColor = Color.Black;
                historyBox.ForeColor = Color.Gray;

                inputBox.BackColor = Color.Black;
                inputBox.ForeColor = Color.Gray;

                userListView.BackColor = Color.Black;
                userListView.ForeColor = Color.Gray;
            }
            else
            {
                historyBox.BackColor = SystemColors.Window;
                historyBox.ForeColor = SystemColors.WindowText;

                inputBox.BackColor = SystemColors.Window;
                inputBox.ForeColor = SystemColors.WindowText;

                userListView.BackColor = SystemColors.Window;
                userListView.ForeColor = SystemColors.WindowText;

            }

        }
        void setupUserList()
        {
            updateUserList(null, circleHash);
            Program.theCore.peerManager.peerRemoved += peerLeft;
            Program.theCore.peerManager.peerAdded += peerJoined;
            Program.theCore.peerManager.peerUpdated += peerUpdated;
            Program.theCore.peerManager.peerRenamed += peerRenamed;
        }
        void peerRenamed(string oldName, Model.Peer p)
        {
            if(p.circles.Contains(circleHash))
                chatReceived("*** " + oldName + " changed name to " + p.username + " at " + DateTime.Now.ToShortTimeString(), circleHash);
            updateUserList(null, circleHash);
        }
        void peerLeft(Model.Peer p, ulong channelId)
        {
            if(channelId == circleHash)
                chatReceived("*** " + p.username + " left at " + DateTime.Now.ToShortTimeString(), circleHash);
            updateUserList(null, circleHash);

        }
        void peerUpdated(Model.Peer p)
        {
            updateUserList(null, circleHash);
        }
        void peerJoined(Model.Peer p, ulong channelId)
        {
            if (channelId == circleHash)
                chatReceived("*** " + p.username + " joined at " + DateTime.Now.ToShortTimeString(), circleHash);

            updateUserList(null, circleHash);
        }
        public void chatReceived(string s, ulong roomId)
        {
            if (roomId != this.circleHash)
                return;
            if (IsDisposed)
            {
                Program.theCore.chatReceivedEvent -= chatReceived;
                return;
            }
            bool highlight = false;

            string z = s;
            if (z.Contains(":"))
                z = z.Substring(z.IndexOf(":")+1);  //timestamp
            if (z.Contains(":"))
                z = z.Substring(z.IndexOf(":") + 1); //user: says this
            if (z.ToLower().Contains(Program.settings.getString("Username", "Username")))
                highlight = true;


            try
            {
                this.Invoke(new Action(delegate ()
                {
                    historyBox.AppendText(s + Environment.NewLine);
                    if (highlight)
                    {
                        historyBox.SelectionStart = historyBox.Text.Length - (s.Length + 1);
                        historyBox.SelectionLength = s.Length + 1;
                        historyBox.SelectionBackColor = SystemColors.Highlight;
                        historyBox.SelectionColor = SystemColors.HighlightText;
                    }
                    updateFont();
                }));
            }
            catch (ObjectDisposedException)
            {

            }
            catch (InvalidOperationException)
            {
            }
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Tab && inputBox.Focused)
            {
                //e.SuppressKeyPress = true;
                string s = inputBox.Text.Substring(0,inputBox.SelectionStart);
                string rest = inputBox.Text.Substring(s.Length);
                if (s.Contains(' ')) s = s.Substring(s.LastIndexOf(' ')+1);
                if (s.Contains('\t')) s = s.Substring(s.LastIndexOf('\t') + 1);
                if (s.Contains('\n')) s = s.Substring(s.LastIndexOf('\n') + 1);

                foreach (Model.Peer p in Program.theCore.peerManager.allPeersInCircle(circleHash))
                {
                    if (p.username.ToLower().StartsWith(s.ToLower()) && s.Trim() != "" && p.username.Trim() != "")
                    {
                        inputBox.Text = inputBox.Text.Substring(0, inputBox.SelectionStart - s.Length) + p.username + " " + rest;
                        inputBox.SelectionStart = inputBox.Text.Length - (rest.Length + 1);
                        return true;
                    }

                }
                System.Media.SystemSounds.Beep.Play();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        private void inputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                if (e.Modifiers != Keys.Shift)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    if (inputBox.Text.Trim() != "")
                    {
                        Program.theCore.sendChat(inputBox.Text, circleHash);
                        inputBox.Text = "";
                        inputBox.Height = 22;
                    }
                }

            }
        }
        private void userListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            foreach (int i in userListView.SelectedIndices)
            {
                Model.Peer z = allPeersInCircle[i];
                TabPage p = new TabPage();
                p.Text = z.username;
                p.Tag = "Files for " + z.id.ToString();
                UserPanel b = new UserPanel(z);
                b.Dock = DockStyle.Fill;
                p.Controls.Add(b);
                Program.mainForm.createOrSelect(p);
            }
        }

        int lastInputBoxHeight = 0;
        int fontHeight = -1;

        private void inputBox_TextChanged(object sender, EventArgs e)
        {
            if (fontHeight == -1)
                fontHeight = inputBox.Font.Height;
            int h = Math.Min(100, fontHeight * (inputBox.Text.Split('\n').Length + 1));
            if (lastInputBoxHeight != h)
            {
                inputBox.Height = h;
                lastInputBoxHeight = h;
            }
        }

        void updateFont()
        {
            historyBox.SelectionStart = 0;
            historyBox.SelectionLength = historyBox.Text.Length;
            historyBox.SelectionFont = Program.getFont();
            historyBox.SelectionStart = historyBox.Text.Length;
            historyBox.ScrollToCaret();
            inputBox.Font = Program.getFont();
        }
        private void CirclePanel_ParentChanged(object sender, EventArgs e)
        {
            updateFont();
        }

        private void historyBox_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }
    }
}
