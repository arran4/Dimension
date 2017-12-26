using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dimension.UI
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
            load();
        }
        void load()
        {
            if (Program.settings.getBool("Use UPnP", true))
            {
                UPnPButton.Checked = true;
                manuallyForwardPortsButton.Checked = false;
            }
            else
            {
                UPnPButton.Checked = false;
                manuallyForwardPortsButton.Checked = true;
            }
            udpDataPortBox.Value = Program.settings.getInt("Default Data Port", 0);
            if (udpDataPortBox.Value == 0)
                udpControlPortBox.Value = 0;
            else
                udpControlPortBox.Value = udpDataPortBox.Value + 1;
        }
        void save()
        {

        }


        private void saveButton_Click(object sender, EventArgs e)
        {
            Program.settings.setBool("Use UPnP", UPnPButton.Checked);
            Program.settings.save();
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
