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
        //FIXME: If you change shares then cancel, it'll still save the share changes but won't trigger an update
        public SettingsForm()
        {
            InitializeComponent();
            load();
        }
        private void usernameBox_TextChanged(object sender, EventArgs e)
        {
            if (Program.comicSansOnly)
            {
                fontSelectBox.Items.Clear();
                fontSelectBox.Items.Add("Comic Sans MS");
                fontSelectBox.SelectedIndex = 0;
            }
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
            dhtPortBox.Value = Program.settings.getInt("Default DHT Port", 0);
            udpControlPortBox.Value = Program.settings.getInt("Default Control Port", Model.NetConstants.controlPort);
            usernameBox.Text = Program.settings.getString("Username", Environment.MachineName);
            descriptionBox.Text = Program.settings.getString("Description", "");

            downloadFolderInput.Text = Program.settings.getString("Default Download Folder", "C:\\Downloads");
            flashNameDropBox.Checked = Program.settings.getBool("Flash on Name Drop", true);
            playSoundsBox.Checked = Program.settings.getBool("Play sounds", true);

            autoRejoinBox.Checked = Program.settings.getBool("Auto Rejoin on Startup", true);
            showAFKBox.Checked = Program.settings.getBool("Show AFK", true);

                fontSelectBox.Text = Program.settings.getString("Font", "Lucida Console");
            if (Program.comicSansOnly)
            {
                fontSelectBox.Items.Clear();
                fontSelectBox.Items.Add("Comic Sans MS");
                fontSelectBox.SelectedIndex = 0;
            }
            useUDTBox.Checked = Program.settings.getBool("Use UDT", true);
            int numShares = Program.fileListDatabase.getInt(Program.settings.settings, "Root Share Count", 0);

            for (int i = 0; i < numShares; i++)
            {
                Model.RootShare r = Program.fileListDatabase.getObject<Model.RootShare>(Program.settings.settings, "Root Share " + i.ToString());

                if (r != null)
                {
                    ListViewItem li = new ListViewItem(r.name);
                    li.SubItems.Add(r.fullPath.Replace('/', System.IO.Path.DirectorySeparatorChar));
                    li.SubItems.Add(ByteFormatter.formatBytes(r.size));
                    li.Tag = r;
                    sharesListView.Items.Add(li);
                }
            }

        }
        void save()
        {
            if (usernameBox.Text.Trim() == "")
                usernameBox.Text = Environment.MachineName;

            Program.settings.setBool("Use UPnP", UPnPButton.Checked);

            Program.settings.setInt("Default Data Port", (int)udpDataPortBox.Value);
            Program.settings.setInt("Default Control Port", (int)udpControlPortBox.Value);
            Program.settings.setInt("Default DHT Port", (int)dhtPortBox.Value);
    
            Program.settings.setString("Username", usernameBox.Text);
            Program.settings.setString("Description", descriptionBox.Text);
            Program.settings.setString("Default Download Folder", downloadFolderInput.Text);

            Program.settings.setBool("Use UDT", useUDTBox.Checked);

            Program.settings.setString("Font", fontSelectBox.Text);

            Program.settings.setBool("Flash on Name Drop", flashNameDropBox.Checked);

            Program.settings.setBool("Play sounds", playSoundsBox.Checked);

            Program.settings.setBool("Show AFK", showAFKBox.Checked);
            Program.settings.setBool("Auto Rejoin on Startup", autoRejoinBox.Checked);

            Program.settings.save();
            
            Close();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            save();
            Program.fileList.startUpdate(false);
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
                //add the folder
                string fullPath = folderBrowserDialog.SelectedPath.Replace('\\', '/');
                string name = fullPath;
                if (name.EndsWith(":/"))
                    name = name.Substring(0, name.IndexOf(":"));
                if (name.Contains("/"))
                    name = name.Substring(name.LastIndexOf("/") + 1);
                bool clear = false;
                while (!clear)
                {
                    clear = true;
                    foreach (Model.RootShare q in Program.fileListDatabase.getRootShares())
                        if(q != null)
                            if (q.name == name)
                                clear = false;
                    if (!clear)
                        name = name + " (2)";

                }

                /*int numShares = Program.fileList.getInt(Program.settings.settings, "Root Share Count", 0);
                for (int i = 0; i < numShares; i++)
                    if (Program.fileList.getObject<Model.RootShare>(Program.settings.settings, "Root Share " + i.ToString()).fullPath.ToLower() == fullPath.ToLower())
                    {
                        MessageBox.Show("You have already added that share.");
                        return;
                    }*/

                Model.RootShare r = new Model.RootShare();
                r.name = name;
                r.fullPath = fullPath;
                r.id = Program.fileListDatabase.allocateId();

                ListViewItem li = new ListViewItem(r.name);
                li.SubItems.Add(r.fullPath.Replace('/', System.IO.Path.DirectorySeparatorChar));
                li.SubItems.Add("");
                li.Tag = r;
                sharesListView.Items.Add(li);

                upsertShare(r);
            }
        }
        void updateSharesNamed(Model.RootShare r, int index)
        {
        }
        void removeSharesNamed(string name, int index)
        {
        }


        void upsertShare(Model.RootShare r)
        {
            int numShares = Program.fileListDatabase.getInt(Program.settings.settings, "Root Share Count", 0);
            for (int i = 0; i < numShares; i++)
            {
                Model.RootShare g = Program.fileListDatabase.getObject<Model.RootShare>(Program.settings.settings, "Root Share " + i.ToString());
                if (g == null)
                {
                    r.index = i;
                    Program.fileListDatabase.setObject<Model.RootShare>(Program.settings.settings, "Root Share " + i.ToString(), r);
                    updateSharesNamed(r, i);
                    return;
                }
                else
                {
                    if (g.fullPath == r.fullPath)
                    {

                        Program.fileListDatabase.setObject<Model.RootShare>(Program.settings.settings, "Root Share " + i.ToString(), r);
                        updateSharesNamed(r, i);
                        return;


                    }
                }
            }

            r.index = numShares;
            Program.fileListDatabase.setObject<Model.RootShare>(Program.settings.settings, "Root Share " + numShares.ToString(), r);

            Program.fileListDatabase.setInt(Program.settings.settings, "Root Share Count", numShares + 1);
            updateSharesNamed(r, numShares);

        }

        private void deleteShareButton_Click(object sender, EventArgs e)
        {
            List<ListViewItem> toRemove = new List<ListViewItem>();
            foreach (ListViewItem i in sharesListView.SelectedItems)
            {
                toRemove.Add(i);

                Model.RootShare r = (Model.RootShare)i.Tag;

                Program.fileListDatabase.setObject<Model.RootShare>(Program.settings.settings, "Root Share " + r.index, null);

                removeSharesNamed(r.name, r.index);
            }
            foreach (ListViewItem i in toRemove)
                sharesListView.Items.Remove(i);
            Program.fileListDatabase.setInt(Program.settings.settings, "Root Share Count", sharesListView.Items.Count);
        }

        private void browseDownloadsButton_Click(object sender, EventArgs e)
        {

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                //add the folder
                string fullPath = folderBrowserDialog.SelectedPath.Replace('\\', '/');
                downloadFolderInput.Text = fullPath;
            }
        }

        private void renameButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem i in sharesListView.SelectedItems)
            {
                RenameShareForm r = new RenameShareForm(i.Text);
                r.ShowDialog();

                Model.RootShare r2 = (Model.RootShare)i.Tag;
                r2.name = r.theName;
                i.Text = r2.name;

                Program.fileListDatabase.setObject<Model.RootShare>(Program.settings.settings, "Root Share " + i.Index, r2);
                
            }
        }
    }
}
