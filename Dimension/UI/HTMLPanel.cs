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
            p.Navigating += navigate;
            Controls.Add(p);
            p.DocumentText = "<html><body><h1>Testing!</h1><p>Oh dear, what hath God wrought...</p> <h2>List of Bootsraps</h2><ul><li><a href='!BOOTSTRAP!http://www.9thcircle.net/Projects/Dimension/bootstrap.php'>9th Circle Test Bootstrap</a></li><li><a href='!BOOTSTRAP!http://www.respawn.com.au/dimension.php'>Respawn LAN Bootstrap</a></li></ul></body></html>";
            p.Refresh(WebBrowserRefreshOption.Completely);
        }
        void navigate(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (e.Url.AbsolutePath.StartsWith("!BOOTSTRAP!"))
            {
                e.Cancel = true;
                string s = e.Url.AbsolutePath;
                s = s.Substring("!BOOTSTRAP!".Length);
                UI.JoinCircleForm.joinCircle(s, JoinCircleForm.CircleType.bootstrap);
            }
            }
        }
}
