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
            if (App.comicSansOnly)
            {
                fontSelectBox.Items.Clear();
                fontSelectBox.Items.Add("Comic Sans MS");
                fontSelectBox.SelectedIndex = 0;
            }
        }
        void load()
        {
            if (App.settings.getBool("Use UPnP", true))
            {
                UPnPButton.Checked = true;
                manuallyForwardPortsButton.Checked = false;
            }
            else
            {
                UPnPButton.Checked = false;
                manuallyForwardPortsButton.Checked = true;
            }
            udpDataPortBox.Value = App.settings.getInt("Default Data Port", 0);
            dhtPortBox.Value = App.settings.getInt("Default DHT Port", 0);
            udpControlPortBox.Value = App.settings.getInt("Default Control Port", Model.NetConstants.controlPort);
            usernameBox.Text = App.settings.getString("Username", Environment.MachineName);
            descriptionBox.Text = App.settings.getString("Description", "");

            downloadFolderInput.Text = App.settings.getString("Default Download Folder", "C:\\Downloads");
            flashNameDropBox.Checked = App.settings.getBool("Flash on Name Drop", true);
            playSoundsBox.Checked = App.settings.getBool("Play sounds", true);

            minimizeToTrayBox.Checked = App.settings.getBool("Minimize to Tray", true);
            autoRejoinBox.Checked = App.settings.getBool("Auto Rejoin on Startup", true);
            showAFKBox.Checked = App.settings.getBool("Show AFK", true);

            updateWithoutPromptingBox.Checked = App.settings.getBool("Update Without Prompting", false);

            reverseDefaultBox.Checked = App.settings.getBool("Default to Reverse Connection", false);
            alwaysRendezvousButton.Checked = App.settings.getBool("Always Rendezvous", false);

            fontSelectBox.Text = App.settings.getString("Font", "Lucida Console");
            if (App.comicSansOnly)
            {
                fontSelectBox.Items.Clear();
                fontSelectBox.Items.Add("Comic Sans MS");
                fontSelectBox.SelectedIndex = 0;
            }
            int numShares = App.fileListDatabase.getInt(App.settings.settings, "Root Share Count", 0);

            for (int i = 0; i < numShares; i++)
            {
                Model.RootShare r = App.fileListDatabase.getObject<Model.RootShare>(App.settings.settings, "Root Share " + i.ToString());

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

            App.settings.setBool("Use UPnP", UPnPButton.Checked);

            App.settings.setInt("Default Data Port", (int)udpDataPortBox.Value);

            if (udpControlPortBox.Value == Model.NetConstants.controlPort)
            {
                MessageBox.Show("UDP Port 31542 is reserved for local broadcasting. Dimension will pick another random port for you.");
                udpControlPortBox.Value = 0;
            }

            App.settings.setInt("Default Control Port", (int)udpControlPortBox.Value);
            App.settings.setInt("Default DHT Port", (int)dhtPortBox.Value);
    
            App.settings.setString("Username", usernameBox.Text);
            App.settings.setString("Description", descriptionBox.Text);
            App.settings.setString("Default Download Folder", downloadFolderInput.Text);
            

            App.settings.setString("Font", fontSelectBox.Text);

            App.settings.setBool("Flash on Name Drop", flashNameDropBox.Checked);
            App.settings.setBool("Minimize to Tray", minimizeToTrayBox.Checked);

            App.settings.setBool("Play sounds", playSoundsBox.Checked);

            App.settings.setBool("Show AFK", showAFKBox.Checked);
            App.settings.setBool("Auto Rejoin on Startup", autoRejoinBox.Checked);
            App.settings.setBool("Update Without Prompting", updateWithoutPromptingBox.Checked);
            
            App.settings.setBool("Always Rendezvous", alwaysRendezvousButton.Checked);
            App.settings.setBool("Default to Reverse Connection", reverseDefaultBox.Checked);

            App.settings.save();
            
            Close();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            save();
            App.fileList.startUpdate(false);
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
                    foreach (Model.RootShare q in App.fileListDatabase.getRootShares())
                        if(q != null)
                            if (q.name == name)
                                clear = false;
                    if (!clear)
                        name = name + " (2)";

                }

                /*int numShares = App.fileList.getInt(App.settings.settings, "Root Share Count", 0);
                for (int i = 0; i < numShares; i++)
                    if (App.fileList.getObject<Model.RootShare>(App.settings.settings, "Root Share " + i.ToString()).fullPath.ToLower() == fullPath.ToLower())
                    {
                        MessageBox.Show("You have already added that share.");
                        return;
                    }*/

                Model.RootShare r = new Model.RootShare();
                r.name = name;
                r.fullPath = fullPath;
                r.id = App.fileListDatabase.allocateId();

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
            int numShares = App.fileListDatabase.getInt(App.settings.settings, "Root Share Count", 0);
            for (int i = 0; i < numShares; i++)
            {
                Model.RootShare g = App.fileListDatabase.getObject<Model.RootShare>(App.settings.settings, "Root Share " + i.ToString());
                if (g == null)
                {
                    r.index = i;
                    App.fileListDatabase.setObject<Model.RootShare>(App.settings.settings, "Root Share " + i.ToString(), r);
                    updateSharesNamed(r, i);
                    return;
                }
                else
                {
                    if (g.fullPath == r.fullPath)
                    {

                        App.fileListDatabase.setObject<Model.RootShare>(App.settings.settings, "Root Share " + i.ToString(), r);
                        updateSharesNamed(r, i);
                        return;


                    }
                }
            }

            r.index = numShares;
            App.fileListDatabase.setObject<Model.RootShare>(App.settings.settings, "Root Share " + numShares.ToString(), r);

            App.fileListDatabase.setInt(App.settings.settings, "Root Share Count", numShares + 1);
            updateSharesNamed(r, numShares);

        }

        private void deleteShareButton_Click(object sender, EventArgs e)
        {
            List<ListViewItem> toRemove = new List<ListViewItem>();
            foreach (ListViewItem i in sharesListView.SelectedItems)
            {
                toRemove.Add(i);

                Model.RootShare r = (Model.RootShare)i.Tag;

                App.fileListDatabase.setObject<Model.RootShare>(App.settings.settings, "Root Share " + r.index, null);

                removeSharesNamed(r.name, r.index);
            }
            foreach (ListViewItem i in toRemove)
                sharesListView.Items.Remove(i);
            App.fileListDatabase.setInt(App.settings.settings, "Root Share Count", sharesListView.Items.Count);
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

                App.fileListDatabase.setObject<Model.RootShare>(App.settings.settings, "Root Share " + i.Index, r2);
                
            }
        }

        private void refreshSharesButton_Click(object sender, EventArgs e)
        {
            App.fileList.clear();
            App.fileList.update(false);
        }
    }
}
