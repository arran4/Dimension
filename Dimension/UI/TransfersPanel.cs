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
        public bool isMono
        {
            get
            {
                return Type.GetType("Mono.Runtime") != null;
            }
        }
        public TransfersPanel()
        {
            InitializeComponent();
            if (!isMono)
            {
                ListView x = listView;
                Controls.Remove(x);
                listView = new DoubleBufferedListView();
                for (int i = 0; i < x.Columns.Count; i++)
                    listView.Columns.Add(x.Columns[i].Text, x.Columns[i].Width);
                listView.View = View.Details;
                listView.Dock = DockStyle.Fill;
                listView.MouseUp += listView_MouseUp;
                listView.MouseDoubleClick += listView_MouseDoubleClick;
                listView.FullRowSelect = true;
                Controls.Add(listView);
            }
            if (isMono)
            {
                listView.OwnerDraw = true;
            }
            }

        string lastFingerprint = "";
        private void updateTimer_Tick(object sender, EventArgs e)
        {
            Model.Transfer[] z;
            lock (Model.Transfer.transfers)
                z = Model.Transfer.transfers.ToArray();

            string fingerprint = "";
            fingerprint += z.Length.ToString();
            foreach (Model.Transfer t in z)
                fingerprint += t.completed.ToString() + t.download.ToString() + t.filename + t.path + t.protocol + t.rate.ToString() + t.size.ToString() + t.username;
            if (lastFingerprint == fingerprint)
                return;
            lastFingerprint = fingerprint;

            if (!isMono)
            {
                listView.BeginUpdate();
            }

            while (listView.Items.Count < z.Length)
                listView.Items.Add(new ListViewItem(""));
            while (listView.Items.Count > z.Length)
                listView.Items.RemoveAt(listView.Items.Count - 1);


            ulong upLimit = Program.settings.getULong("Global Upload Rate Limit", 0);
            ulong downLimit = Program.settings.getULong("Global Download Rate Limit", 0);
            for (int i = 0; i < z.Length; i++)
            {
                string[] w = new string[10];
                w[0] = z[i].filename;
                if (z[i].download)
                    w[1] = "Downloading";
                else
                    w[1] = "Uploading";
                w[2] = z[i].username;

                string percent = ((int)((100.0 * z[i].completed) / z[i].size)).ToString() + "%";
                string limit = "None";
                if (z[i].download)
                {
                    if (downLimit > 0)
                        limit = UI.ByteFormatter.formatBytes(downLimit) + "/s";
                }
                else
                {
                    if (upLimit > 0)
                        limit = UI.ByteFormatter.formatBytes(upLimit) + "/s";
                }
                if (z[i].con != null)
                {
                    if (z[i].con is Model.IncomingConnection)
                    {
                        if (((Model.IncomingConnection)z[i].con).rateLimiterDisabled)
                            limit = "Bypassed";
                    }
                    else if (z[i].con is Model.OutgoingConnection)
                    {
                        if (((Model.OutgoingConnection)z[i].con).rateLimiterDisabled)
                            limit = "Bypassed";
                    }
                }

                string eta;
                var timeElapsed = DateTime.Now.Subtract(z[i].timeCreated);
                float fraction=0;
                if(z[i].completed > 0 && z[i].size > 0)
                    fraction = z[i].completed / (float)z[i].size;
                double seconds;

                if (fraction == 0)
                    seconds = 0;
                else
                    seconds = (timeElapsed.TotalSeconds * (1.0f/fraction))- timeElapsed.TotalSeconds;
                TimeSpan timeSpan = new TimeSpan(0, 0, (int)seconds);
                eta = timeSpan.ToString();


                w[3] = ByteFormatter.formatBytes(z[i].rate) + "/s";
                w[4] = limit;
                w[5] = eta;
                w[6] = ByteFormatter.formatBytes(z[i].completed);
                w[7] = percent;
                w[8] = ByteFormatter.formatBytes(z[i].size);
                w[9] = z[i].protocol;

                while (listView.Items[i].SubItems.Count < w.Length)
                    listView.Items[i].SubItems.Add("");
                for (int x = 0; x < 9; x++)
                    if (listView.Items[i].SubItems[x].Text != w[x])
                        listView.Items[i].SubItems[x].Text = w[x];
                listView.Items[i].Tag = z[i];
            }


            if (!isMono)
            {
                listView.EndUpdate();
            }
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
                        t.thePeer.transfers.Remove(t.originalPath);
                }
                lock(Model.Transfer.transfers)
                    Model.Transfer.transfers.Remove(t);
                t.con.send(new Model.Commands.CancelCommand() { path = t.originalPath });
            }
            }

        private void disableLimitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem i in listView.SelectedItems)
            {
                Model.Transfer t = (Model.Transfer)i.Tag;

                if (t.con != null)
                {
                    if (t.con is Model.IncomingConnection)
                        ((Model.IncomingConnection)t.con).rateLimiterDisabled = true;
                    else if (t.con is Model.OutgoingConnection)
                        ((Model.OutgoingConnection)t.con).rateLimiterDisabled = true;
                }
            }
        }

        private void enableLimitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem i in listView.SelectedItems)
            {
                Model.Transfer t = (Model.Transfer)i.Tag;

                if (t.con != null)
                {
                    if (t.con is Model.IncomingConnection)
                        ((Model.IncomingConnection)t.con).rateLimiterDisabled = false;
                    else if (t.con is Model.OutgoingConnection)
                        ((Model.OutgoingConnection)t.con).rateLimiterDisabled = false;
                }
            }
        }

        private void listView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView.SelectedItems.Count > 0)
            {
                Model.Transfer t = (Model.Transfer)listView.SelectedItems[0].Tag;
                foreach (Model.Peer p in Program.theCore.peerManager.allPeers)
                    if (p.id== t.userId)
                    {
                        Program.mainForm.selectUser(p);
                        return;
                    }
            }
        }

        private void listView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {

            if (e.Bounds.Left > listView.Width)
                return;
            e.Graphics.DrawString(e.SubItem.Text, listView.Font, new SolidBrush(SystemColors.ControlText), e.Bounds.Location);
        }

        private void listView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            if (e.Bounds.Left > listView.Width)
                return;
            e.Graphics.DrawString(e.Item.Text, listView.Font, new SolidBrush(SystemColors.ControlText), e.Bounds.Location);

        }

        private void listView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {

            if (e.Bounds.Left > listView.Width)
                return;

            int width = e.Bounds.Width;
            if (e.Bounds.Right > listView.Width)
                width = listView.Width;
            e.Graphics.FillRectangle(new SolidBrush(SystemColors.Control), new RectangleF(e.Bounds.Left, e.Bounds.Top, width, e.Bounds.Height));
            e.Graphics.DrawString(e.Header.Text, listView.Font, new SolidBrush(SystemColors.ControlText), e.Bounds.Location);
        }
    }
}
