using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dimension
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using(System.Threading.Mutex mutex = new System.Threading.Mutex(false, "Global\\DimensionMutex"))
            {
                App.settings = new Model.Settings();
#if DEBUG
#else
                if (checkForUpdates())
                    return;
#endif
                if (!mutex.WaitOne(0, false))
                {
                    MessageBox.Show("Dimension is already running. Please check your task manager to make sure you've closed it fully before running.");
                    return;
                }


                string tempFolder = System.IO.Path.GetTempPath();
                string updaterPath = System.IO.Path.Combine(tempFolder, System.IO.Path.Combine("DimensionTemp", "Updater.exe"));
                if (System.IO.File.Exists(updaterPath) && System.IO.File.Exists("Updater.exe"))
                {
                    System.Threading.Thread.Sleep(1000);
                    try
                    {
                        System.IO.File.Delete("Updater.exe");
                        System.IO.File.Copy(updaterPath, "Updater.exe");
                    }
                    catch
                    {
                        //Probably not elevated, oh well no updater update for you
                    }
                }
                if (System.IO.Directory.Exists(System.IO.Path.Combine(tempFolder, "DimensionTemp")))
                    System.IO.Directory.Delete(System.IO.Path.Combine(tempFolder, "DimensionTemp"), true);

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

#if DEBUG
#else
            try
            {
#endif
                Application.Run(new LoadingForm());

                App.mainForm = new MainForm();
                Application.Run(App.mainForm);
                App.doCleanup();
#if DEBUG
#else
            }
            catch (Exception e)
            {
                Model.SystemLog.addEntry("Exception!");
                Model.SystemLog.addEntry(e.Message);
                Model.SystemLog.addEntry(e.StackTrace);
                return;
            }
#endif
            }
        }
    }
}
