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

        //TODO: Make it not scroll down if you've already gone up
        public SystemLogPanel()
        {
            InitializeComponent();

            update();
        }

        void updateFont()
        {
            contentBox.Font = Program.getFont();
        }
        void update()
        {
            contentBox.Text = Model.SystemLog.theLog;
            contentBox.SelectionStart = contentBox.Text.Length;
            contentBox.SelectionLength = 0;
            contentBox.ScrollToCaret();
            updateFont();
        }
        private void updateTimer_Tick(object sender, EventArgs e)
        {
            if (contentBox.Text != Model.SystemLog.theLog)
            {
                update();
            }
        }

        private void SystemLogPanel_ParentChanged(object sender, EventArgs e)
        {
            updateFont();
        }
    }
}
