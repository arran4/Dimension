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
    public partial class FileBrowserPanel : UserControl, Model.ClosableTab
    {
        public void close()
        {
            p.commandReceivedEvent -= commandReceived;
        }
        Model.Peer p;
        public FileBrowserPanel(Model.Peer p)
        {
            InitializeComponent();
            this.p = p;
            p.commandReceivedEvent += commandReceived;
            p.createConnection();
            p.controlConnection.send(new Model.Commands.GetFileListing("/"));
        }
        void doUpdate(Model.Commands.FileListing list)
        {
            ignoreReselect = true;
            foldersView.BeginUpdate();
            foldersView.Nodes.Clear();
            TreeNode root = new TreeNode("/");
            root.Name = "/";
            root.Text = p.username;
            foldersView.Nodes.Add(root);
            foreach (Model.Commands.FSListing i in list.folders)
            {
                root.Nodes.Add(new TreeNode(i.name));
            }
            foldersView.EndUpdate();
            filesView.BeginUpdate();
            filesView.Items.Clear();

            foreach (Model.Commands.FSListing i in list.folders)
            {
                ListViewItem l = new ListViewItem();
                l.Text = i.name;
                l.Name = i.name;
                l.SubItems.Add(ByteFormatter.formatBytes(i.size));
                filesView.Items.Add(l);
            }
            filesView.EndUpdate();
            ignoreReselect = false;
        }
        string currentPath = "/";
        void commandReceived(Model.Commands.Command c)
        {
            if (c is Model.Commands.FileListing)
            {
                Model.Commands.FileListing f = (Model.Commands.FileListing)c;
                if (f.path == currentPath)
                {
                    if (this.InvokeRequired)
                        this.Invoke(new Action(delegate () { doUpdate(f); }));
                    else
                        doUpdate(f);
                }

            }
        }
        bool ignoreReselect = false;
        private void foldersView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (ignoreReselect)
                return;
            string s = "";

            TreeNode t = e.Node;
            s = e.Node.Name;
            
            while (t.Parent != null)
            {
                if(s == "")
                    s = t.Text;
                else
                    s = t.Text + "/" + s;
                t = t.Parent;
            }
            if(s != "/") s = "/" + s;
            if (s != currentPath)
            {
                currentPath = s;
                p.controlConnection.send(new Model.Commands.GetFileListing(currentPath));
            }
        }
    }
}
