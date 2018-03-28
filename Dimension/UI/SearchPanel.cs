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
                this.Invoke(new Action(delegate
                {
                    resultsBox.BeginUpdate();
                }));
                foreach (Dimension.Model.Commands.FSListing f in c.folders)
                {
                    ListViewItem i = new ListViewItem();
                    i.Text = f.name;
                    i.SubItems.Add(ByteFormatter.formatBytes(f.size));
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
    }
}
