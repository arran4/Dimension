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
        static bool disposed = false;
        static void doCleanup()
        {
            disposed = true;
            bootstrap.Dispose();
        }
        public static Udt.Socket udtSocket;
        public static Model.Bootstrap bootstrap;
        public static void doLoad()
        {
            bootstrap = new Model.Bootstrap();
            bootstrap.launch().Wait();
            udtSocket = bootstrap.udtSocket;
            udtSocket.Listen(int.MaxValue);
            System.Threading.Thread t = new System.Threading.Thread(acceptLoop);
            t.IsBackground = true;
            t.Name = "UDT Accept Loop";
            t.Start();

        }

        static void acceptLoop()
        {
            while (!disposed)
            {
                Udt.Socket u = udtSocket.Accept();


            }


            }
        }
}
