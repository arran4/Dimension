using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheArtOfDev.HtmlRenderer.WinForms;

namespace Dimension.UI
{
    public partial class HTMLPanel : UserControl
    {
        public HTMLPanel()
        {
            InitializeComponent();

            WebBrowser p = new WebBrowser();
            p.Dock = DockStyle.Fill;
            Controls.Add(p);
            p.DocumentText = "<h1>Testing!</h1><p>Oh dear, what hath God wrought...</p>";
            p.Refresh(WebBrowserRefreshOption.Completely);
        }
    }
}
