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
            foldersView.BeginUpdate();
            foldersView.Nodes.Clear();
            foreach (Model.Commands.FSListing i in list.folders)
            {
                foldersView.Nodes.Add(new TreeNode(i.name));
            }
            foldersView.EndUpdate();
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
    }
}
