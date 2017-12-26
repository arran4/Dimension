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

        private void joinButton_Click(object sender, EventArgs e2)
        {
            string s = urlBox.Text;
            this.Close();
            var t = new System.Threading.Thread(delegate ()
            {
                System.Net.IPEndPoint[] e = Program.bootstrap.join(s);
                Program.mainForm.Invoke(new Action(delegate ()
                {
                    Program.mainForm.addInternetCircle(e, s);

                }));
            })
            {
                Name = "Bootstrap join thread",
                IsBackground = true
            };
            t.Start();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
