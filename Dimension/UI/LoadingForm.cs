using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dimension
{
    public partial class LoadingForm : Form
    {
        public LoadingForm()
        {
            InitializeComponent();
        }

        bool started = false;
        private void loadTimer_Tick(object sender, EventArgs e)
        {
            if (!started)
            {
                started = true;
                System.Threading.Thread t = new System.Threading.Thread(Program.doLoad);
                t.Name = "Loading Thread";
                t.Start();
            }
            label1.Text = "Dimension - " + Program.currentLoadState;
            if (Program.doneLoading)
                this.Close();
            }
    }
}
