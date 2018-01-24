using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Principal;

namespace Updater
{
    public partial class UpdatingForm : Form
    {
        public UpdatingForm(string url)
        {
            InitializeComponent();
            label1.Text = "Waiting for Dimension to close...";
            System.Threading.Thread t = new System.Threading.Thread(delegate ()
            {

                using (System.Threading.Mutex mutex = new System.Threading.Mutex(false, "Global\\DimensionMutex"))
                {
                    try
                    {
                        while (!mutex.WaitOne(0, false))
                            System.Threading.Thread.Sleep(100);
                    }
                    catch
                    {
                    }

                    this.Invoke(new Action(delegate ()
                    {
                        label1.Text = "Downloading new version of Dimension...";
                    }));
                    try
                    {
                        System.Net.WebClient w = new System.Net.WebClient();
                        byte[] b = w.DownloadData("http://9cstatic.nfshost.com/Dimension/" + url);
                        string tempFolder = System.IO.Path.GetTempPath();



                        this.Invoke(new Action(delegate ()
                        {
                            label1.Text = "Installing new version of Dimension...";
                        }));

                        if (System.IO.File.Exists(System.IO.Path.Combine(tempFolder, "newversion.zip")))
                            System.IO.File.Delete(System.IO.Path.Combine(tempFolder, "newversion.zip"));
                        System.IO.File.WriteAllBytes(System.IO.Path.Combine(tempFolder, "newversion.zip"), b);
                        if (System.IO.Directory.Exists(System.IO.Path.Combine(tempFolder, "DimensionTemp")))
                            System.IO.Directory.Delete(System.IO.Path.Combine(tempFolder, "DimensionTemp"), true);
                        System.IO.Directory.CreateDirectory(System.IO.Path.Combine(tempFolder, "DimensionTemp"));
                        System.IO.Compression.ZipFile.ExtractToDirectory(System.IO.Path.Combine(tempFolder, "newversion.zip"), System.IO.Path.Combine(tempFolder, "DimensionTemp"));
                        foreach (System.IO.FileInfo f in new System.IO.DirectoryInfo(System.IO.Path.Combine(tempFolder, "DimensionTemp")).GetFiles())
                            if (f.Name != "Updater.exe")
                                System.IO.File.Copy(f.FullName, f.Name, true);


                        if (System.IO.File.Exists(System.IO.Path.Combine(tempFolder, "newversion.zip")))
                            System.IO.File.Delete(System.IO.Path.Combine(tempFolder, "newversion.zip"));



                        this.Invoke(new Action(delegate ()
                        {
                            label1.Text = "Launching new version of Dimension...";
                        }));

                        if (!isAdmin())
                            System.Diagnostics.Process.Start("Dimension.exe");
                        Application.Exit();
                    }
                    catch (AccessViolationException)
                    {
                        elevate();
                    }
                    catch (UnauthorizedAccessException)
                    {
                        elevate();
                    }
                }
            });
            t.IsBackground = true;
            t.Name = "Download thread";
            t.Start();
        }
        static void elevate()
        {

            if (isAdmin())
            {
                MessageBox.Show("Error - could not access temp folder. Failing...");
                Application.Exit();
                return;

            }
            else
            {
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.FileName = System.Reflection.Assembly.GetEntryAssembly().Location;
                p.StartInfo.Verb = "runas";
                p.StartInfo.UseShellExecute = true;
                p.Start();

                p.WaitForExit();
                System.Diagnostics.Process.Start("Dimension.exe");
                Application.Exit();

            }
        }
        static bool isAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        }
}
