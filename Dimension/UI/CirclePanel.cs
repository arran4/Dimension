﻿using System;
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
                else if(circleType == JoinCircleForm.CircleType.kademlia)
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

            circleHash = BitConverter.ToUInt64(sha.ComputeHash(Encoding.UTF8.GetBytes(url)), 0);
            Program.theCore.joinCircle(url);
            Program.theCore.chatReceivedEvent += chatReceived;


            System.Threading.Thread t = new System.Threading.Thread(joinLoop);
            t.IsBackground = true;
            t.Name = "Circle join loop";
            t.Start();
        }
        ulong circleHash;
        Model.Peer[] allPeersInCircle
        {
            get
            {
                return Program.theCore.peerManager.allPeersInCircle(circleHash);
            }
        }
        void updateUserList(Model.Peer p)
        {
            var items = allPeersInCircle;
            if (items.Length == 0)
                return;
            if (items.Length < userListView.VirtualListSize)
            {
                if (this.InvokeRequired)
                {
                    Invoke(new Action(delegate ()
                    {
                        userListView.VirtualListSize = items.Length;
                        userListView.RedrawItems(0, items.Length-1, false);

                    }));
                }
                else
                {
                    userListView.VirtualListSize = items.Length;
                    userListView.RedrawItems(0, items.Length-1, false);
                }
                return;
            }
            if (p != null)
                for (int i = 0; i < items.Length; i++)
                    if (items[i].id == p.id)
                    {
                        if (this.InvokeRequired)
                        {
                            Invoke(new Action(delegate ()
                            {
                                userListView.VirtualListSize = items.Length;
                                userListView.RedrawItems(i, i, false);
                            }));
                        }
                        else
                        {
                            userListView.VirtualListSize = items.Length;
                            userListView.RedrawItems(i, i, false);
                        }
                        return;
                    }
        }
        public CirclePanel()
        {
            InitializeComponent();
            setupUserList();

            url = "LAN";
            Program.theCore.joinCircle(url);
            System.Security.Cryptography.SHA512Managed sha = new System.Security.Cryptography.SHA512Managed();

            circleHash = BitConverter.ToUInt64(sha.ComputeHash(Encoding.UTF8.GetBytes(url)), 0);
            Program.theCore.chatReceivedEvent += chatReceived;
        }
        void setupUserList()
        {
            updateUserList(null);
            Program.theCore.peerManager.peerRemoved += updateUserList;
            Program.theCore.peerManager.peerAdded += updateUserList;
            Program.theCore.peerManager.peerRenamed += updateUserList;
        }
        private void userListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            ListViewItem i = new ListViewItem();
            i.Tag = allPeersInCircle[e.ItemIndex];
            i.Text = allPeersInCircle[e.ItemIndex].username;
            i.SubItems.Add(allPeersInCircle[e.ItemIndex].buildNumber.ToString());
            i.SubItems.Add(ByteFormatter.formatBytes(allPeersInCircle[e.ItemIndex].share));
            e.Item = i;
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
            try
            {
                this.Invoke(new Action(delegate ()
                {
                    historyBox.Text += s + Environment.NewLine;
                    updateFont();
                }));
            }
            catch (ObjectDisposedException)
            {

            }
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

        private void inputBox_TextChanged(object sender, EventArgs e)
        {
            inputBox.Height = Math.Min(100, inputBox.Font.Height * (inputBox.Text.Split('\n').Length+1));
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
    }
}
