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
    public partial class TransfersPanel : UserControl
    {
        public TransfersPanel()
        {
            InitializeComponent();
            ListView x = listView;
            Controls.Remove(x);
            listView = new DoubleBufferedListView();
            for (int i = 0; i < x.Columns.Count; i++)
                listView.Columns.Add(x.Columns[i].Text,x.Columns[i].Width);
            listView.View = View.Details;
            listView.Dock = DockStyle.Fill;
            listView.MouseUp += listView_MouseUp;
            listView.FullRowSelect = true;
            Controls.Add(listView);
        }

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            listView.BeginUpdate();

            Model.Transfer[] z;
            lock (Model.Transfer.transfers)
                z = Model.Transfer.transfers.ToArray();

            while (listView.Items.Count < z.Length)
                listView.Items.Add(new ListViewItem(""));
            while (listView.Items.Count > z.Length)
                listView.Items.RemoveAt(listView.Items.Count - 1);


            for (int i = 0; i < z.Length; i++)
            {
                string[] w = new string[8];
                w[0] = z[i].filename;
                if (z[i].download)
                    w[1] = "Downloading";
                else
                    w[1] = "Uploading";
                w[2] = z[i].username;

                string percent = ((int)((100.0 * z[i].completed) / z[i].size)).ToString() + "%";

                w[3] = ByteFormatter.formatBytes(z[i].rate) + "/s";
                w[4] = ByteFormatter.formatBytes(z[i].completed);
                w[5] = percent;
                w[6] = ByteFormatter.formatBytes(z[i].size);
                w[7] = z[i].protocol;

                while (listView.Items[i].SubItems.Count < w.Length)
                    listView.Items[i].SubItems.Add("");
                for (int x = 0; x < 8; x++)
                    if (listView.Items[i].SubItems[x].Text != w[x])
                        listView.Items[i].SubItems[x].Text = w[x];
                listView.Items[i].Tag = z[i];
            }


            listView.EndUpdate();
        }

        private void listView_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && listView.SelectedItems.Count > 0)
            {
                contextMenuStrip1.Show(Cursor.Position);

            }
            }

        private void cancelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem i in listView.SelectedItems)
            {
                Model.Transfer t = (Model.Transfer)i.Tag;
                if (t.thePeer != null)
                {
                    lock(t.thePeer.transfers)
                        t.thePeer.transfers.Remove(t.path);
                }
                lock(Model.Transfer.transfers)
                    Model.Transfer.transfers.Remove(t);
                t.con.send(new Model.Commands.CancelCommand() { path = t.path });
            }
            }
    }
}
