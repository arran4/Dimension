using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dimension
{
    static class Program
    {
        public static MainForm mainForm;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new LoadingForm());

            mainForm = new MainForm();
            Application.Run(mainForm);
            doCleanup();
        }
        static bool disposed = false;
        static void doCleanup()
        {
            disposed = true;
            bootstrap.Dispose();
        }
        public static System.Net.Sockets.UdpClient udp;
        public static Udt.Socket udtSocket;
        public static Model.Bootstrap bootstrap;
        public static Model.FileList fileList;
        public static Model.Settings settings;
        public static void doLoad()
        {
            settings = new Model.Settings();

            currentLoadState = "Setting up NAT...";
            bootstrap = new Model.Bootstrap();
            bootstrap.launch().Wait();
            udtSocket = bootstrap.udtSocket;
            udtSocket.Listen(int.MaxValue);
            udp = bootstrap.unreliableClient;
            doReceive();
            System.Threading.Thread t = new System.Threading.Thread(acceptLoop);
            t.IsBackground = true;
            t.Name = "UDT Accept Loop";
            t.Start();
            fileList = new Model.FileList();
            doneLoading = true;
        }
        public static bool doneLoading = false;

        static void doReceive()
        {
            udp.BeginReceive(receiveCallback, null);
            //TODO: Catch socket exception on receive
        }
        static void receiveCallback(IAsyncResult ar)
        {
            System.Net.IPEndPoint sender = null;
            byte[] data = udp.EndReceive(ar, ref sender);
            //TODO: Parse
            //TODO: Catch socket exception on receive

            doReceive();
        }
        public static string currentLoadState = "";
        public static List<Model.IncomingConnection> incomingConnections = new List<Model.IncomingConnection>();

        //TODO: Remove old incoming connections when they're dead
        static void acceptLoop()
        {
            while (!disposed)
            {
                Udt.Socket u = udtSocket.Accept();
                Model.IncomingConnection c = new Model.IncomingConnection(u);
                lock (incomingConnections)
                    incomingConnections.Add(c);
            }


        }
    }
}
