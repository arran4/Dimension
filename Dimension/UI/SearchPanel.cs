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
    public partial class SearchPanel : UserControl
    {
        public SearchPanel()
        {
            InitializeComponent();
            Program.theCore.searchResult += searchCallback;
        }
        void searchCallback(Model.Commands.SearchResultCommand c)
        {
            if (c.keyword == keyword)
            {
                try
                {
                    this.Invoke(new Action(delegate
                    {
                        resultsBox.BeginUpdate();
                    }));
                }
                catch
                {
                    return; //disposed
                }
                string username = "";
                foreach (var v in Program.theCore.peerManager.allPeers)
                    if (v.id == c.myId)
                    {
                        username = v.username;
                        break;
                    }
                foreach (Dimension.Model.Commands.FSListing f in c.folders)
                {
                    ListViewItem i = new ListViewItem();
                    i.Text = f.name;
                    i.SubItems.Add(ByteFormatter.formatBytes(f.size));
                    i.SubItems.Add(username);
                    i.Tag = new SearchThingy() { fsListing = f, userId = c.myId };
                    this.Invoke(new Action(delegate
                    {
                        resultsBox.Items.Add(i);
                    }));
                }
                foreach (Dimension.Model.Commands.FSListing f in c.files)
                {
                    ListViewItem i = new ListViewItem();
                    i.Text = f.name;
                    i.SubItems.Add(ByteFormatter.formatBytes(f.size));
                    i.SubItems.Add(username);
                    i.Tag = new SearchThingy() { fsListing = f, userId = c.myId };
                    this.Invoke(new Action(delegate
                    {
                        resultsBox.Items.Add(i);
                    }));
                }
                this.Invoke(new Action(delegate
                {
                    resultsBox.EndUpdate();
                }));

            }
        }
        string keyword;

        private void searchButton_Click(object sender, EventArgs e)
        {
            doSearch();
        }
        void doSearch()
        {
            resultsBox.Items.Clear();
            Model.Commands.KeywordSearchCommand c = new Model.Commands.KeywordSearchCommand();
            c.keyword = searchInputBox.Text;
            keyword = c.keyword;
            Program.theCore.beginSearch(c);
        }
        private void searchInputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                doSearch();
            }
        }

        private void resultsBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            List<SearchThingy> toDownload = new List<SearchThingy>();
            foreach (ListViewItem z in resultsBox.SelectedItems)
            {
                var t = (SearchThingy)z.Tag;
                toDownload.Add(t);
            }
                System.Threading.Thread t2 = new System.Threading.Thread(delegate ()
            {
                foreach (SearchThingy q in toDownload)
                {
                    foreach (Model.Peer p in Program.theCore.peerManager.allPeers)
                        if (p.id == q.userId)
                        {
                            if (p.controlConnection == null || p.dataConnection == null)
                                p.createConnection();

                            //current folder = "/" + q.fsListing.fullPath.Substring(0,q.fsListing.fullPath.LastIndexOf("/"))
                            p.downloadElement(q.fsListing.fullPath, q.fsListing);

                            break;
                        }
                }

                
            });
            t2.IsBackground = true;
            t2.Name = "Search download trigger thread";
            t2.Start();
        }
    }
    class SearchThingy
    {
        public Model.Commands.FSListing fsListing;
        public ulong userId;

    }
}
