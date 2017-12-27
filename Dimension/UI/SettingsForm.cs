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



            int numShares = Program.fileList.getInt(Program.fileList.fileList, "Root Share Count", 0);

            for (int i = 0; i < numShares; i++)
            {
                Model.RootShare r = Program.fileList.getObject<Model.RootShare>(Program.fileList.fileList, "Root Share " + i.ToString());

                ListViewItem li = new ListViewItem(r.name);
                li.SubItems.Add(r.fullPath);
                li.Tag = r;
                sharesListView.Items.Add(li);
            }

        }
        void save()
        {
            Program.settings.setBool("Use UPnP", UPnPButton.Checked);

            Program.settings.setInt("Default Data Port", (int)udpDataPortBox.Value);
            Program.settings.setInt("Default Control Port", (int)udpControlPortBox.Value);

            Program.settings.setString("Username", usernameBox.Text);
            Program.settings.setString("Description",descriptionBox.Text);

            Program.settings.save();
            Program.fileList.saveAll();
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

        private void addShareButton_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.SelectedPath = "";
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                int numShares = Program.fileList.getInt(Program.fileList.fileList, "Root Share Count", 0);
                //add the folder
                string fullPath = folderBrowserDialog.SelectedPath.Replace('\\', '/');
                string name = fullPath;
                if (name.Contains("/"))
                    name = name.Substring(name.IndexOf("/") + 1);

                Model.RootShare r = new Model.RootShare();
                r.name = name;
                r.fullPath = fullPath;

                ListViewItem li = new ListViewItem(name);
                li.SubItems.Add(fullPath);
                li.Tag = r;
                sharesListView.Items.Add(li);

                Program.fileList.setObject<Model.RootShare>(Program.fileList.fileList, "Root Share " + numShares.ToString(), r);
                int[] z = Program.fileList.getObject<int[]>(Program.fileList.fileList, "Root Shares Named " + name);
                if (z == null)
                    z = new int[0];
                Array.Resize(ref z, z.Length + 1);
                z[z.Length - 1] = numShares;
                Program.fileList.setObject<int[]>(Program.fileList.fileList, "Root Shares Named " + name, z);

                Program.fileList.setInt(Program.fileList.fileList, "Root Share Count", numShares + 1);
            }
        }
    }
}
