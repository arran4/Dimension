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
    public partial class JoinCircleForm : Form
    {
        public JoinCircleForm()
        {
            InitializeComponent();
        }

        private void joinButton_Click(object sender, EventArgs e)
        {
            string s = urlBox.Text;
            this.Close();
            System.Threading.Thread t = new System.Threading.Thread(delegate()
            {
                Program.chord.join(s);
            });
            t.IsBackground = true;
            t.Start();

        }
    }
}
