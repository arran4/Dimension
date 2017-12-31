﻿using System;
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
                w[2] = "User";
                w[3] = "TimeLeft";
                w[4] = "Speed";
                w[5] = ByteFormatter.formatBytes(z[i].completed);
                w[6] = ByteFormatter.formatBytes(z[i].size);
                w[7] = z[i].protocol;

                while (listView.Items[i].SubItems.Count < w.Length)
                    listView.Items[i].SubItems.Add("");
                for (int x = 0; x < 8; x++)
                    if (listView.Items[i].SubItems[x].Text != w[x])
                        listView.Items[i].SubItems[x].Text = w[x];
            }


            listView.EndUpdate();
        }
    }
}
