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
    public partial class SystemLogPanel : UserControl
    {

        //TODO: Make it scroll down when new entries are added
        //TODO: But not if we've scrolled up
        public SystemLogPanel()
        {
            InitializeComponent();
            contentBox.Text = Model.SystemLog.theLog;
        }

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            if (contentBox.Text != Model.SystemLog.theLog)
                contentBox.Text = Model.SystemLog.theLog;
        }
    }
}
