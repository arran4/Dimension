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
        }

        private void searchButton_Click(object sender, EventArgs e)
        {
            doSearch();
        }
        void doSearch()
        {
            Model.Commands.KeywordSearchCommand c = new Model.Commands.KeywordSearchCommand();
            c.keyword = searchInputBox.Text;
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
