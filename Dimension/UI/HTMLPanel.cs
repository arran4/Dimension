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
    public partial class HTMLPanel : UserControl
    {

        public static bool isMono
        {
            get
            {
                return Type.GetType("Mono.Runtime") != null;
            }
        }

        public HTMLPanel()
        {
            InitializeComponent();
            
            var p = new TheArtOfDev.HtmlRenderer.WinForms.HtmlPanel();
            p.Dock = DockStyle.Fill;
            p.LinkClicked += navigate;
            Controls.Add(p);
            p.Text = "<html><body><h1>Welcome to Dimension!</h1><p>It's still young, so there aren't many networks to join. Check these ones out:</p> <h2>List of Bootstraps</h2><ul><li><a href='DimensionBootstrap://www.9thcircle.net/Projects/Dimension/bootstrap.php'>9th Circle Test Bootstrap</a></li><li><a href='DimensionBootstrap://www.respawn.com.au/dimension.php'>Respawn LAN Bootstrap</a></li><li><a href='DimensionLAN://LAN'>Your local network</a></li></ul></body></html>"; 
        }
        void navigate(object sender, TheArtOfDev.HtmlRenderer.Core.Entities.HtmlLinkClickedEventArgs e)
        {
            if (e.Link.ToLower().StartsWith("dimensionbootstrap://"))
            {
                e.Handled = true;
                string s = e.Link;
                s = "http://" + s.Substring("DimensionBootstrap://".Length);
                UI.JoinCircleForm.joinCircle(s, JoinCircleForm.CircleType.bootstrap);
            }
            if (e.Link.ToLower().StartsWith("dimensionlan://"))
            {
                e.Handled = true;
                string s = e.Link;
                s = "http://" + s.Substring("DimensionLAN://".Length);
                UI.JoinCircleForm.joinCircle(s, JoinCircleForm.CircleType.LAN);
            }
        }
        }
}
