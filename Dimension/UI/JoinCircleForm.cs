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
        public enum CircleType
        {
            bootstrap,
            kademlia
        }
        CircleType circleType;
        public JoinCircleForm(CircleType circleType)
        {
            this.circleType = circleType;
            InitializeComponent();
            if (circleType == CircleType.kademlia)
            {

                urlBox.Text = "#Test";
                if (!Program.kademlia.ready)
                {
                    joinButton.Enabled = false;
                    kadInitLabel.Visible = true;
                }
            }
        }

        private void joinButton_Click(object sender, EventArgs e2)
        {
            string s = urlBox.Text;
            this.Close();
            var t = new System.Threading.Thread(delegate ()
            {
            System.Net.IPEndPoint[] e;
                if (circleType == CircleType.bootstrap)
                {
                    e = Program.bootstrap.join(s);
                }
                else
                {
                    e = new System.Net.IPEndPoint[] { };
                    //takes too long, display the thing later
                    //e = Program.kademlia.doLookup(s.ToLower().Trim());
                }
                Program.mainForm.Invoke(new Action(delegate ()
                {
                    Program.mainForm.addInternetCircle(e, s, circleType);

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

        private void kadInitLabelTimer_Tick(object sender, EventArgs e)
        {
            if (circleType == CircleType.kademlia)
            {
                
                if (Program.kademlia.ready)
                {
                    joinButton.Enabled =true;
                    kadInitLabel.Visible = false;
                }
            }
        }
    }
}
