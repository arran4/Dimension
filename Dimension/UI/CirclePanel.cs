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
        }
        void updateUserList(Model.Peer p)
        {
            var items = Program.theCore.peerManager.allPeers;
            userListView.VirtualListSize = items.Length;
            if (p != null)
                for (int i = 0; i < items.Length; i++)
                    if (items[i].id == p.id)
                    {
                        if (this.InvokeRequired)
                            Invoke(new Action(delegate (){userListView.RedrawItems(i, i, false);}));
                        else
                            userListView.RedrawItems(i, i, false);
                        return;
                    }
        }
        public CirclePanel()
        {
            InitializeComponent();
            setupUserList();
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
            i.Tag = Program.theCore.peerManager.allPeers[e.ItemIndex];
            i.Text = Program.theCore.peerManager.allPeers[e.ItemIndex].username;
            e.Item = i;
        }
    }
}
