using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace Dimension.UI
{
    public partial class CirclePanel : UserControl, Model.ClosableTab, UI.SelectableTab
    {
        public void close()
        {
            Program.theCore.leaveCircle(url);
            if (url == "LAN")
                Program.settings.setBool("Joined LAN Circle", false);
            if (circleType == JoinCircleForm.CircleType.bootstrap)
                Program.settings.removeStringToArrayNoDup("Bootstrap Circles Open", url);
            if (circleType == JoinCircleForm.CircleType.kademlia)
                Program.settings.removeStringToArrayNoDup("Kademlia Circles Open", url);
        }
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
            InitializeComponent();
            setupUserList();

            if (url.ToLower() == "http://lan/" || circleType == JoinCircleForm.CircleType.LAN)
            {
                initLAN();
                return;
            }
            this.circleType = circleType;
            this.url = url;

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

            if (circleType == JoinCircleForm.CircleType.bootstrap)
            {
                Program.settings.addStringToArrayNoDup("Bootstrap Circles Open", url);
            }
            else if (circleType == JoinCircleForm.CircleType.kademlia)
            {
                Program.settings.addStringToArrayNoDup("Kademlia Circles Open", url);
            }
            if (isMono)
            {
                userListView.OwnerDraw = true;
            }
        }
        public bool isMono
        {
            get
            {
                return Type.GetType("Mono.Runtime") != null;
            }
        }
        public CirclePanel()
        {
            InitializeComponent();
            setupUserList();
            initLAN();
        }
        void initLAN()
        {
            url = "LAN";
            Program.theCore.joinCircle(url);
            System.Security.Cryptography.SHA512Managed sha = new System.Security.Cryptography.SHA512Managed();

            circleHash = BitConverter.ToUInt64(sha.ComputeHash(Encoding.UTF8.GetBytes(url.ToLower())), 0);
            Program.theCore.chatReceivedEvent += chatReceived;
            MainForm.colorChange += colorChanged;
            Program.mainForm.setColors();

            Program.settings.setBool("Joined LAN Circle", true);
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
                else
                    userListView.Items[i].ForeColor = SystemColors.WindowText;
                Font b = userListView.Font;
                if (items[i].behindDoubleNAT)
                    b = new Font("Comic Sans MS", b.SizeInPoints);
                
                if (items[i].probablyDead)
                    userListView.Items[i].Font = new Font(b, FontStyle.Italic);
                else
                    userListView.Items[i].Font = new Font(b, FontStyle.Regular);
               

                if (items[i].afk.HasValue)
                    userListView.Items[i].Text = s2 + (items[i].afk.Value ? " (AFK)" : "");
                else
                    userListView.Items[i].Text = s2;
                userListView.Items[i].SubItems[1].Text = (items[i].buildNumber.ToString());
                userListView.Items[i].SubItems[2].Text = (ByteFormatter.formatBytes(items[i].share));
                userListView.Items[i].SubItems[3].Text = (items[i].description);
            }
            userListView.EndUpdate();
        }
        bool addedEvent = false;
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
                    fingerprint += p2.username + p2.share.ToString() + p2.buildNumber.ToString() + p2.afk.ToString() + p2.probablyDead.ToString() + p2.maybeDead.ToString()+p2.description + p2.behindDoubleNAT + p2.quit + p2.timeQuit;
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
                chatReceived("*** " + oldName + " changed name to " + p.username + " at " + DateTime.Now.ToShortTimeString(), circleHash,p);
            updateUserList(null, circleHash);
        }
        void peerLeft(Model.Peer p, ulong channelId, bool notify)
        {
            if(channelId == circleHash && notify)
                chatReceived("*** " + p.username + " left at " + DateTime.Now.ToShortTimeString(), circleHash,p);
            updateUserList(null, circleHash);

        }
        void peerUpdated(Model.Peer p)
        {
            updateUserList(null, circleHash);
        }
        void peerJoined(Model.Peer p, ulong channelId, bool notify)
        {
            if (channelId == circleHash)
                chatReceived("*** " + p.username + " joined at " + DateTime.Now.ToShortTimeString(), circleHash,p);

            updateUserList(null, circleHash);
        }
        List<string> chatHistory = new List<string>();
        int chatHistorySelection = 0;
        public void chatReceived(string s, ulong roomId, Model.Peer p)
        {
            if (Parent != null && Parent.Parent != null && !addedEvent)
            {
                addedEvent = true;
                
            }
            if (roomId != this.circleHash)
                return;
            if (IsDisposed)
            {
                Program.theCore.chatReceivedEvent -= chatReceived;
                return;
            }
            bool highlight = false;

            int count = new Regex(Regex.Escape(Program.settings.getString("Username", "Username").ToLower())).Matches(s).Count;
            if (p.id == Program.theCore.id)
            {
                if (count > 1)
                    highlight = true;
            }
            else
            {
                if (count > 0)
                    highlight = true;
            }


            try
            {
                this.Invoke(new Action(delegate ()
                {
                    if (!focused && addedEvent && changeEventReceived)
                    {
                        TabPage p2 = (TabPage)Parent;
                        if (!p2.Text.StartsWith("(!) "))
                            p2.Text = "(!) " + p2.Text;
                    }
                    historyBox.AppendText(s + Environment.NewLine);
                    if (highlight)
                    {
                        historyBox.SelectionStart = historyBox.Text.Length - (s.Length + 1);
                        historyBox.SelectionLength = s.Length;
                        historyBox.SelectionBackColor = SystemColors.Highlight;
                        historyBox.SelectionColor = SystemColors.HighlightText;
                        Program.mainForm.flash();
                    }

                    historyBox.SelectionStart = historyBox.Text.Length - (s.Length + 1);
                    historyBox.SelectionLength = s.Length + 1;
                    historyBox.SelectionFont = Program.getFont();
                    
                    if (p != null)
                        if (p.behindDoubleNAT)
                        {
                            historyBox.SelectionStart = historyBox.Text.Length - (s.Length + 1);
                            historyBox.SelectionLength = s.Length + 1;
                            historyBox.SelectionFont = new Font("Comic Sans MS", historyBox.Font.SizeInPoints);
                        }
                    if (selected)
                    {
                        historyBox.SelectionStart = historyBox.Text.Length;
                        historyBox.SelectionLength = 0;
                        historyBox.ScrollToCaret();
                    }

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
                if (Program.settings.getBool("Play sounds", true))
                {
                    System.Media.SoundPlayer p = new System.Media.SoundPlayer(Properties.Resources.Bell);
                    p.Play();
                }
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        private void inputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (fontHeight == -1)
                fontHeight = inputBox.Font.Height;
            int h = Math.Min(100, fontHeight * (inputBox.Text.Split('\n').Length + 1));
            if (lastInputBoxHeight != h)
            {
                inputBox.Height = h;
                lastInputBoxHeight = h;
            }
            if (inputBox.Lines.Length <= 1) {
                if (e.KeyCode == Keys.Up)
                {
                    int n = chatHistorySelection + 1;
                    if (n > 0 && n <= chatHistory.Count)
                    {
                        string s = chatHistory[chatHistory.Count - n];
                        inputBox.Text = s;
                        chatHistorySelection = n;
                    }
                    else
                    {
                        inputBox.Text = "";
                        chatHistorySelection = chatHistory.Count;
                    }
                }
                if (e.KeyCode == Keys.Down)
                {
                    int n = chatHistorySelection - 1;
                    if (n > 0 && n <= chatHistory.Count)
                    {
                        string s = chatHistory[(chatHistory.Count) - n];
                        inputBox.Text = s;
                        chatHistorySelection = n;
                    }
                    else
                    {
                        inputBox.Text = "";
                        chatHistorySelection = 0;
                    }
                }
            }
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                if (e.Modifiers != Keys.Shift)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    if (inputBox.Text.Trim() != "")
                    {
                        chatHistorySelection = 0;
                        chatHistory.Add(inputBox.Text);
                        while (chatHistory.Count > 10)
                            chatHistory.RemoveAt(0);
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
                UserPanel b = new UserPanel(z);
                b.Dock = DockStyle.Fill;

                Program.mainForm.addOrSelectPanel(z.username, b, "(!) Files for " + z.id.ToString());
            }
        }

        int lastInputBoxHeight = 0;
        int fontHeight = -1;
        
        private void CirclePanel_ParentChanged(object sender, EventArgs e)
        {
        }
        bool changeEventReceived = false;
        public bool focused = false;

        private void historyBox_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(e.LinkText);
            }
            catch (Win32Exception)
            {
                //WINE throws this sometimes, doesn't actually do anything
            }
        }
        bool selected = false;
        public void select()
        {
            selected = true;
            

            historyBox.Visible = true;
            historyBox.SelectionStart = historyBox.Text.Length;
            historyBox.SelectionLength = 0;
            historyBox.ScrollToCaret();
            historyBox.Refresh();
        }
        public void unselect()
        {
            selected = false;
            historyBox.Visible = false;
        }
        private void userListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            if (!selected)
                return;
            if (e.Bounds.Left > userListView.Width)
                return;
            e.Graphics.DrawString(e.Item.Text, userListView.Font, new SolidBrush(SystemColors.ControlText), e.Bounds.Location);

        }

        private void userListView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            if (!selected)
                return;
            if (e.Bounds.Left > userListView.Width)
                return;

            int width = e.Bounds.Width;
            if (e.Bounds.Right > userListView.Width)
                width = userListView.Width;
            e.Graphics.FillRectangle(new SolidBrush(SystemColors.Control), new RectangleF(e.Bounds.Left, e.Bounds.Top, width, e.Bounds.Height) );
            e.Graphics.DrawString(e.Header.Text, userListView.Font, new SolidBrush(SystemColors.ControlText), e.Bounds.Location);
        }

        private void userListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            if (!selected)
                return;
            if (e.Bounds.Left > userListView.Width)
                return;
            e.Graphics.DrawString(e.SubItem.Text, userListView.Font, new SolidBrush(SystemColors.ControlText), e.Bounds.Location);
        }
    }
}
