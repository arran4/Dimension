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
    public partial class LimitChangeDialog : Form
    {
        public enum WhichLimit
        {
            up,
            down
        }
        WhichLimit whichLimit;
        public LimitChangeDialog(WhichLimit whichLimit)
        {
            this.whichLimit = whichLimit;
            InitializeComponent();
            if (whichLimit == WhichLimit.up)
                typeLabel.Text = "Global Upload Rate Limit:";
            if (whichLimit == WhichLimit.down)
                typeLabel.Text = "Global Download Rate Limit:";


            ulong value = 0;
            if (whichLimit == WhichLimit.up)
                value = Program.settings.getULong("Global Upload Rate Limit", 0);
            if (whichLimit == WhichLimit.down)
                value = Program.settings.getULong("Global Download Rate Limit", 0);

            unitComboBox.SelectedIndex = 0;
            if (value == 0)
                noLimitButton.Checked = true;
            else
            {
                yesLimitButton.Checked = true;
                while (value > 1024)
                {
                    unitComboBox.SelectedIndex++;
                    value /= 1024;
                }
                valueBox.Value = value;
            }

        }
        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void okayButton_Click(object sender, EventArgs e)
        {
            ulong value=0;

            if (noLimitButton.Checked)
                value = 0;
            if (yesLimitButton.Checked)
            {
                value = (ulong) valueBox.Value;
                for (int i = 0; i < unitComboBox.SelectedIndex; i++)
                    value *= 1024;
            }

            if (whichLimit == WhichLimit.up)
                Program.settings.setULong("Global Upload Rate Limit", value);
            if (whichLimit == WhichLimit.down)
                Program.settings.setULong("Global Download Rate Limit", value);
            this.Close();
        }

        private void valueBox_ValueChanged(object sender, EventArgs e)
        {
            yesLimitButton.Checked = true;
        }

        private void unitComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

            yesLimitButton.Checked = true;
        }

        private void valueBox_KeyDown(object sender, KeyEventArgs e)
        {
            yesLimitButton.Checked = true;
        }
    }
}
