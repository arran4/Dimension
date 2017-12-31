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
            System.Threading.Thread t = new System.Threading.Thread(delegate ()
            {
                p.createConnection();
                while (p.controlConnection == null)
                    System.Threading.Thread.Sleep(10);
                p.controlConnection.send(new Model.Commands.GetFileListing("/"));
            });
            t.Name = "Create connection thread";
            t.IsBackground = true;
            t.Start();
        }
        TreeNode traverse(string path)
        {
            TreeNode current;
            if (foldersView.Nodes.Count == 0)
            {
                TreeNode root = new TreeNode("/");
                root.Name = "/";
                root.Text = p.username;
                foldersView.Nodes.Add(root);
            }
            current = foldersView.Nodes[0];

            string[] z = path.Split('/');
            if (z.Length > 1)
            {
                for (int i = 1; i < z.Length; i++)
                {
                    foreach (TreeNode n in current.Nodes)
                        if (n.Name == z[i])
                        {
                            current = n;
                            break;
                        }
                }
            }
            return current;
        }
        void doUpdate(Model.Commands.FileListing list)
        {
            ignoreReselect = true;
            foldersView.BeginUpdate();
            //foldersView.Nodes.Clear();
            TreeNode root = traverse(list.path);
            root.Nodes.Clear();
            foreach (Model.Commands.FSListing i in list.folders)
            {
                TreeNode t = new TreeNode(i.name);
                t.Name = i.name;
                root.Nodes.Add(t);
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
                l.Tag = i;
                filesView.Items.Add(l);
            }
            foreach (Model.Commands.FSListing i in list.files)
            {
                ListViewItem l = new ListViewItem();
                l.Text = i.name;
                l.Name = i.name;
                l.SubItems.Add(ByteFormatter.formatBytes(i.size));
                l.Tag = i;
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
            s = "";
            
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

        private void filesView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (filesView.SelectedItems.Count > 0)
            {
                Model.Commands.FSListing tag = (Model.Commands.FSListing)filesView.SelectedItems[0].Tag;
                if (tag.isFolder)
                {
                    if (currentPath == "/")
                        currentPath += tag.name;
                    else
                        currentPath += "/" + tag.name;
                    p.controlConnection.send(new Model.Commands.GetFileListing(currentPath));
                }
                else
                {

                    string s;
                    if (currentPath == "/")
                        s = "/" + tag.name;
                    else
                        s = currentPath + "/" + tag.name;
                    bool useUDT = false;

                    if (p.udtConnection != null && p.useUDT && Program.settings.getBool("Use UDT", true))
                        useUDT = true;
                    if (p.t == null)
                    {

                        p.t = new Model.Transfer();
                        p.t.username = p.username;
                        p.t.filename = tag.name;
                        p.t.download = true;
                        p.t.size = tag.size;
                        p.t.completed = 0;
                        if (p.dataConnection is Model.LoopbackOutgoingConnection)
                            p.t.protocol = "Loopback";
                        else
                            if (useUDT)
                                p.t.protocol = "UDT";
                            else
                                p.t.protocol = "TCP";

                        lock (Model.Transfer.transfers)
                            Model.Transfer.transfers.Add(p.t);
                    }
                    System.Threading.Thread t = new System.Threading.Thread(delegate ()
                    {
                        if (useUDT)
                            p.udtConnection.send(new Model.Commands.RequestChunks() { allChunks = true, path = s });
                        else
                            p.dataConnection.send(new Model.Commands.RequestChunks() { allChunks = true, path = s });
                    });
                    t.IsBackground = true;
                    t.Name = "Download request thread";
                    t.Start();
                }

                }
        }
    }
}
