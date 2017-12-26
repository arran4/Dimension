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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            doLoad();
            Application.Run(new MainForm());
            doCleanup();
        }
        static void doCleanup()
        {
            chord.Dispose();
        }
        public static Model.Bootstrap chord;
        public static void doLoad()
        {
            chord = new Model.Bootstrap();
            chord.launch().Wait();
        }
        }
}
