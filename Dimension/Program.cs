using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dimension
{
    static class Program
    {
        static void threadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            Model.SystemLog.addEntry("Exception!");
            Model.SystemLog.addEntry(e.Exception.Message);
            Model.SystemLog.addEntry(e.Exception.StackTrace);
        }
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
                if (App.checkForUpdates())
                    return;
#endif
                if (!mutex.WaitOne(0, false))
                {
                    MessageBox.Show("Dimension is already running. Please check your task manager to make sure you've closed it fully before running.");
                    return;
                }

                Application.ThreadException += threadException;
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
