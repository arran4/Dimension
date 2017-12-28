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

            circleHash = BitConverter.ToInt32(sha.ComputeHash(Encoding.UTF8.GetBytes(url)), 0);
            Program.theCore.joinCircle(url);
        }
        int circleHash;
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

            circleHash = BitConverter.ToInt32(sha.ComputeHash(Encoding.UTF8.GetBytes(url)), 0);
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
            e.Item = i;
        }
    }
}
