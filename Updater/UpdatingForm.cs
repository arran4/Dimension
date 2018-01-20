using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Updater
{
    public partial class UpdatingForm : Form
    {
        public UpdatingForm(string url)
        {
            InitializeComponent();
            System.Threading.Thread t = new System.Threading.Thread(delegate ()
            {
                System.Net.WebClient w = new System.Net.WebClient();
                byte[] b = w.DownloadData("http://9cstatic.nfshost.com/Dimension/" + url);

                if (System.IO.File.Exists("newversion.zip"))
                    System.IO.File.Delete("newversion.zip");
                System.IO.File.WriteAllBytes("newversion.zip", b);
                if (System.IO.Directory.Exists("temp"))
                    System.IO.Directory.Delete("temp", true);
                System.IO.Directory.CreateDirectory("temp");
                System.IO.Compression.ZipFile.ExtractToDirectory("newversion.zip", "temp");
                foreach (System.IO.FileInfo f in new System.IO.DirectoryInfo("temp").GetFiles())
                    if (f.Name != "Updater.exe")
                        System.IO.File.Copy(f.FullName, f.Name, true);


                if (System.IO.Directory.Exists("temp"))
                    System.IO.Directory.Delete("temp",true);
                if (System.IO.File.Exists("newversion.zip"))
                    System.IO.File.Delete("newversion.zip");
                System.Diagnostics.Process.Start("Dimension.exe");
                Application.Exit();
            });
            t.IsBackground = true;
            t.Name = "Download thread";
            t.Start();
        }
    }
}
