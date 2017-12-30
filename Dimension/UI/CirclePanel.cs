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
        public string url;
        public CirclePanel(string url)
        {
            this.url = url;
            InitializeComponent();
            setupUserList();

            System.Security.Cryptography.SHA512Managed sha = new System.Security.Cryptography.SHA512Managed();

            circleHash = BitConverter.ToUInt64(sha.ComputeHash(Encoding.UTF8.GetBytes(url)), 0);
            Program.theCore.joinCircle(url);
            Program.theCore.chatReceivedEvent += chatReceived;
        }
        ulong circleHash;
        Model.Peer[] allPeersInCircle
        {
            get
            {
                var startList = Program.theCore.peerManager.allPeers;
                List<Model.Peer> output = new List<Model.Peer>();
                foreach (var v in startList)
                    if (v.circles.Contains(circleHash))
                        output.Add(v);
                return output.ToArray();
            }
        }
        void updateUserList(Model.Peer p)
        {
            var items = allPeersInCircle;
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
            Program.theCore.peerManager.peerAdded += updateUserList;
            Program.theCore.peerManager.peerRenamed += updateUserList;
        }
        private void userListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            ListViewItem i = new ListViewItem();
            i.Tag = allPeersInCircle[e.ItemIndex];
            i.Text = allPeersInCircle[e.ItemIndex].username;
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
            this.Invoke(new Action(delegate ()
            {
                historyBox.Text += s + Environment.NewLine;
                historyBox.SelectionStart = historyBox.Text.Length;
                historyBox.ScrollToCaret();
            }));
            }
        private void inputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                Program.theCore.sendChat(inputBox.Text, circleHash);
                inputBox.Text = "";

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
                FileBrowserPanel b = new FileBrowserPanel();
                b.Dock = DockStyle.Fill;
                p.Controls.Add(b);
                Program.mainForm.createOrSelect(p);
            }
            }
    }
}
