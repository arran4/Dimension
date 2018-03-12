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
    public partial class UserPanel : UserControl, Model.ClosableTab
    {
        public void close()
        {
            p.commandReceivedEvent -= commandReceived;
        }
        void updateFont()
        {
            historyBox.Font = Program.getFont();
            inputBox.Font = Program.getFont();
        }
        public void selectChat()
        {
            tabControl1.SelectedIndex = 1;
        }
        Model.Peer p;
        public UserPanel(Model.Peer p)
        {
            InitializeComponent();
            filesView.SmallImageList = iconCache;
            this.p = p;
            p.commandReceivedEvent += commandReceived;
            displayMessage("Attempting TCP connection to " + p.actualEndpoint.Address.ToString());
            System.Threading.Thread t = new System.Threading.Thread(delegate ()
            {
                p.createConnection(displayMessage);
                while (p.controlConnection == null)
                    System.Threading.Thread.Sleep(10);
                displayMessage("Successfully set up connection.");
                p.controlConnection.send(new Model.Commands.GetFileListing("/"));
            });
            t.Name = "Create connection thread";
            t.IsBackground = true;
            t.Start();
            updateFont();
        }
        static ImageList iconCache = new ImageList();

        bool connected = false;
        public void displayMessage(string s)
        {
            if (this.InvokeRequired)
            {
                try
                {
                    this.Invoke(new Action(delegate ()
                    {
                        ListViewItem i = new ListViewItem(s);
                        filesView.Items.Add(i);
                    }));
                }
                catch (InvalidOperationException)
                {
                }
            }
            else
            {
                ListViewItem i = new ListViewItem(s);
                filesView.Items.Add(i);
            }
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
            connected = true;
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

            if (list.path != "/")
            {
                filesView.Items.Add(new ListViewItem(".."));
            }

            foreach (Model.Commands.FSListing i in list.folders)
            {
                ListViewItem l = new ListViewItem();
                l.Text = i.name;
                l.Name = i.name;
                l.SubItems.Add(ByteFormatter.formatBytes(i.size));
                l.Tag = i;
                filesView.Items.Add(l);

                try
                {
                    if (!iconCache.Images.ContainsKey("Folder"))
                        iconCache.Images.Add("Folder", IconReader.GetFolderIcon(IconReader.IconSize.Small, IconReader.FolderType.Closed));
                    l.ImageKey = "Folder";
                }
                catch
                {
                    //don't care
                }
            }
            foreach (Model.Commands.FSListing i in list.files)
            {
                ListViewItem l = new ListViewItem();
                l.Text = i.name;
                l.Name = i.name;
                l.SubItems.Add(ByteFormatter.formatBytes(i.size));
                l.Tag = i;
                filesView.Items.Add(l);

                string s = i.name;
                if (i.name.Contains("."))
                    s = s.Substring(s.LastIndexOf(".") + 1);

                try
                {
                    if (!iconCache.Images.ContainsKey(s))
                        iconCache.Images.Add(s, IconReader.GetFileIcon("filename." + s, IconReader.IconSize.Small, false));
                    l.ImageKey = s;
                }
                catch
                {
                    //don't care
                }
            }
            filesView.EndUpdate();
            ignoreReselect = false;
            updateFilterList();
        }
        string currentPath = "/";
        void commandReceived(Model.Commands.Command c)
        {

            if (c is Model.Commands.PrivateChatCommand)
            {
                Model.Commands.PrivateChatCommand chat = (Model.Commands.PrivateChatCommand)c;

                foreach (string s in chat.content.Split('\n'))
                {
                    string w = DateTime.Now.ToShortTimeString() + " " + p.username + ": " + s;
                    try
                    {
                        if (this.InvokeRequired)
                            this.Invoke(new Action(delegate () { addLine(w); }));
                        else
                            addLine(w);
                    }
                    catch (InvalidOperationException)
                    {
                    }
                }
            }
            if (c is Model.Commands.FileListing)
            {
                Model.Commands.FileListing f = (Model.Commands.FileListing)c;
                lastFileListing = f;
                if (f.path == currentPath)
                {
                    try
                    {
                        if (this.InvokeRequired)
                            this.Invoke(new Action(delegate () { doUpdate(f); }));
                        else
                            doUpdate(f);
                    }
                    catch (InvalidOperationException)
                    {
                    }
                }

            }
        }
        Model.Commands.FileListing lastFileListing=null;
        public void addLine(string s)
        {
            historyBox.Text += s + Environment.NewLine;
            historyBox.SelectionStart = historyBox.Text.Length;
            historyBox.ScrollToCaret();
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
                if (s == "")
                    s = t.Text;
                else
                    s = t.Text + "/" + s;
                t = t.Parent;
            }
            if (s != "/") s = "/" + s;
            if (s != currentPath)
            {
                currentPath = s;
                p.controlConnection.send(new Model.Commands.GetFileListing(currentPath));
            }
        }

        private void filesView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (!connected)
                return;
            if (filesView.SelectedItems.Count > 0)
            {
                if (filesView.SelectedItems[0].Text == "..")
                {
                    filesView.Items.Clear();
                    currentPath = currentPath.TrimEnd('/');
                    currentPath = currentPath.Substring(0, currentPath.LastIndexOf('/'));
                    if (currentPath == "")
                        currentPath = "/";
                    p.controlConnection.send(new Model.Commands.GetFileListing(currentPath));
                    return;
                }
                Model.Commands.FSListing tag = (Model.Commands.FSListing)filesView.SelectedItems[0].Tag;
                if (tag.isFolder)
                {
                    filesView.Items.Clear();
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
                    downloadElement(s, tag);
                }
            }

        }

        void downloadElement(string s, Model.Commands.FSListing tag)
        {

            bool useUDT = false;
            
            Model.Transfer t;
            lock (p.transfers)
            {
                if (!p.transfers.ContainsKey(s))
                {
                    t = new Model.Transfer();
                    t.thePeer = p;
                    p.transfers[s] = t;
                    t.originalPath = s;
                    t.path = s;
                    t.username = p.username;
                    t.userId = p.id;
                    t.filename = tag.name;
                    t.download = true;
                    t.size = tag.size;
                    t.completed = 0;
                    if (p.dataConnection is Model.LoopbackOutgoingConnection)
                        t.protocol = "Loopback";
                    else
                        if (p.dataConnection is Model.ReliableOutgoingConnection)
                            t.protocol = "TCP";
                        else
                            t.protocol = "UDT";

                    p.transfers[t.path] = t;
                    lock (Model.Transfer.transfers)
                        Model.Transfer.transfers.Add(t);
                }
                else
                    t = p.transfers[s];
            }
            System.Threading.Thread t2 = new System.Threading.Thread(delegate ()
            {
                long startingByte = 0;
                string downloadPath = Model.Peer.downloadFilePath(s);
                if (System.IO.File.Exists(downloadPath + ".incomplete"))
                {
                    startingByte = new System.IO.FileInfo(downloadPath + ".incomplete").Length;
                    t.completed = (ulong)startingByte;
                }
                if (startingByte >= (long)tag.size) //we already have the file
                {
                    p.transfers.Remove(t.path);
                    lock (Model.Transfer.transfers)
                        Model.Transfer.transfers.Remove(t);
                    return;
                }
                Model.Commands.Command c;
                if (tag.isFolder)
                {
                    c = new Model.Commands.RequestFolderContents { path = s };
                }
                else
                {
                    c = new Model.Commands.RequestChunks() { allChunks = true, path = s, startingByte = startingByte };
                }
                t.con = p.dataConnection;
                p.dataConnection.send(c);
                
            });
            t2.IsBackground = true;
            t2.Name = "Download request thread";
            t2.Start();
        }

        private void inputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                if (inputBox.Text.Trim() != "" && e.Modifiers != Keys.Shift)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;


                    if (p.controlConnection == null)
                        addLine("Error - haven't connected to other peer yet.");
                    else
                    {
                        if(p.id != Program.theCore.id)
                            p.controlConnection.send(new Model.Commands.PrivateChatCommand() { content = inputBox.Text });


                        foreach (string s in inputBox.Text.Split('\n'))
                        {
                            string w = DateTime.Now.ToShortTimeString() + " " + Program.settings.getString("Username", "Username") + ": " + s;
                            if (this.InvokeRequired)
                                this.Invoke(new Action(delegate () { addLine(w); }));
                            else
                                addLine(w);
                        }
                        inputBox.Text = "";
                        inputBox.Height = 22;
                    }
                }

            }
        }

        private void inputBox_TextChanged(object sender, EventArgs e)
        {
            int h = Math.Min(100, inputBox.Font.Height * (inputBox.Text.Split('\n').Length + 1));
            if (inputBox.Height != h)
                inputBox.Height = h;
        }

        private void FileBrowserPanel_ParentChanged(object sender, EventArgs e)
        {
            updateFont();
        }

        private void filesView_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && filesView.SelectedItems.Count > 0)
            {
                contextMenuStrip1.Show(Cursor.Position);
            }
        }

        private void downloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem i in filesView.SelectedItems)
            {
                Model.Commands.FSListing tag = (Model.Commands.FSListing)i.Tag;
                string s;
                if (currentPath == "/")
                    s = "/" + tag.name;
                else
                    s = currentPath + "/" + tag.name;
                downloadElement(s, tag);
            }
        }

        void updateFilterList()
        {
            foreach (ListViewItem i in filesView.Items)
            {
                if (filterBox.Text.Trim() == "")
                    i.ForeColor = SystemColors.WindowText;
                else
                    if (i.Text.ToLower().Contains(filterBox.Text.ToLower()))
                        i.ForeColor = SystemColors.WindowText;
                    else
                        i.ForeColor = SystemColors.GrayText;
            }
            }

        private void filterBox_TextChanged(object sender, EventArgs e)
        {
            updateFilterList();
        }

        bool haveRendered = false;
        private void firstRenderTimer_Tick(object sender, EventArgs e)
        {
            if (!haveRendered && lastFileListing != null)
            {
                firstRenderTimer.Enabled = false;
                haveRendered = true;
                doUpdate(lastFileListing);
            }
        }
    }
}