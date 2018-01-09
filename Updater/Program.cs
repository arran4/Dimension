using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Updater
{
    public static class Program
    {
        public static bool needsUpdate(int currentVersion)
        {
            string s = (new System.IO.StreamReader(System.Net.WebRequest.CreateHttp(latestPath).GetResponse().GetResponseStream())).ReadToEnd();

            int latestVersion = int.Parse(s.Split('\n')[0]);
            if (latestVersion <= currentVersion)
                return false;
            else
                return true;
        }
        public static string downloadPath()
        {
            string s = (new System.IO.StreamReader(System.Net.WebRequest.CreateHttp(latestPath).GetResponse().GetResponseStream())).ReadToEnd();
            return s.Split('\n')[1];
        }
        public const string latestPath = "http://9cstatic.nfshost.com/Dimension/latest.txt";
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            using (System.Threading.Mutex mutex = new System.Threading.Mutex(false, "Global\\DimensionUpdateMutex"))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                Application.Run(new UpdatingForm(downloadPath()));
            }
        }
    }
}
