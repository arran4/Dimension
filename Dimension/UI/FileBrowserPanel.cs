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
    public partial class FileBrowserPanel : UserControl
    {
        Model.Peer p;
        public FileBrowserPanel(Model.Peer p)
        {
            this.p = p;
            p.createConnection();
            p.controlConnection.send(new Model.Commands.GetFileListing("/"));
            InitializeComponent();
        }
    }
}
