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
    public partial class RenameShareForm : Form
    {
        public string theName;
        public RenameShareForm(string theName)
        {
            InitializeComponent();
            this.theName = theName;
            textBox1.Text = theName;
        }

        private void okayButton_Click(object sender, EventArgs e)
        {
            theName = textBox1.Text;
            Close();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;

                theName = textBox1.Text;
                Close();
            }
        }

        private void RenameShareForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;

                theName = textBox1.Text;
                Close();
            }
        }
    }
}
