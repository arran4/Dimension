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
            fileListDatabase.saveAll();
            bootstrap.Dispose();
        }
        public static Model.Core theCore;
        public static System.Net.Sockets.UdpClient udp;
        public static Udt.Socket udtSocket;
        public static Model.Bootstrap bootstrap;
        public static Model.FileListDatabase fileListDatabase;
        public static Model.Settings settings;
        public static Model.FileList fileList;
        public static Model.Serializer serializer;
        public static void doLoad()
        {
            settings = new Model.Settings();


            string username = settings.getString("Username", Environment.MachineName);
            settings.setString("Username", username);

            currentLoadState = "Setting up NAT...";
            bootstrap = new Model.Bootstrap();
            bootstrap.launch().Wait();
            udtSocket = bootstrap.udtSocket;
            udtSocket.Listen(int.MaxValue);
            udp = bootstrap.unreliableClient;
            System.Threading.Thread t = new System.Threading.Thread(acceptLoop);
            t.IsBackground = true;
            t.Name = "UDT Accept Loop";
            t.Start();
            fileListDatabase = new Model.FileListDatabase();

            fileList = new Model.FileList();
            
            fileList.startUpdate(false);

            serializer = new Model.Serializer();
            theCore = new Model.Core();

            doneLoading = true;
        }
        public static bool doneLoading = false;

        public static string currentLoadState = "";
        public static List<Model.IncomingConnection> incomingConnections = new List<Model.IncomingConnection>();

        //TODO: Remove old incoming connections when they're dead
        static void acceptLoop()
        {
            while (!disposed)
            {
                Udt.Socket u = udtSocket.Accept();
                Model.IncomingConnection c = new Model.UdtIncomingConnection(u);
                lock (incomingConnections)
                    incomingConnections.Add(c);
            }


        }
    }
}
