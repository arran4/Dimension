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
            udpControlPortBox.Value = Program.settings.getInt("Default Control Port", 0);
            usernameBox.Text = Program.settings.getString("Username", Environment.MachineName);
            descriptionBox.Text= Program.settings.getString("Description", "");
        }
        void save()
        {
            Program.settings.setBool("Use UPnP", UPnPButton.Checked);

            Program.settings.setInt("Default Data Port", (int)udpDataPortBox.Value);
            Program.settings.setInt("Default Control Port", (int)udpControlPortBox.Value);

            Program.settings.setString("Username", usernameBox.Text);
            Program.settings.setString("Description",descriptionBox.Text);

            Program.settings.save();
            Close();
        }
        
        private void saveButton_Click(object sender, EventArgs e)
        {
            save();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
