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
        public static Model.ByteCounter globalUpCounter = new Model.ByteCounter();
        public static Model.ByteCounter globalDownCounter = new Model.ByteCounter();
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using(System.Threading.Mutex mutex = new System.Threading.Mutex(false, "Global\\DimensionMutex"))
            {
                if (!mutex.WaitOne(0, false))
                {
                    MessageBox.Show("Dimension is already running. Please check your task manager to make sure you've closed it fully before running.");
                    return;
                }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

#if DEBUG
#else
            try
            {
#endif
                Application.Run(new LoadingForm());

                mainForm = new MainForm();
                Application.Run(mainForm);
                doCleanup();
#if DEBUG
#else
            }
            catch (Exception e)
            {
                Model.SystemLog.addEntry("Exception!");
                Model.SystemLog.addEntry(e.Message);
                Model.SystemLog.addEntry(e.StackTrace);
            }
#endif
            }
        }
        static bool disposed = false;
        static void doCleanup()
        {
            disposed = true;
            kademlia.Dispose();
            theCore.Dispose();
            fileList.Dispose();
            fileListDatabase.close();
            bootstrap.Dispose();
            Model.SystemLog.writer.Close();
        }
        public static Model.Kademlia kademlia;
        public static Model.Core theCore;
        public static System.Net.Sockets.UdpClient udp;
        public static System.Net.Sockets.TcpListener listener;
        public static Model.Bootstrap bootstrap;
        public static Model.FileListDatabase fileListDatabase;
        public static Model.Settings settings;
        public static Model.FileList fileList;
        public static Model.Serializer serializer;
        public static void doLoad()
        {
#if DEBUG
#else
            try
            {
#endif
            settings = new Model.Settings();


            string username = settings.getString("Username", Environment.MachineName);
            settings.setString("Username", username);

            currentLoadState = "Setting up NAT...";
            bootstrap = new Model.Bootstrap();
            bootstrap.launch().Wait();
            listener = bootstrap.listener;
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

            t = new System.Threading.Thread(delegate ()
            {
                kademlia = new Model.Kademlia();
                kademlia.initialize();
            });
            t.IsBackground = true;
            t.Name = "Kademlia Init Thread";
            t.Start();

            doneLoading = true;
#if DEBUG
#else
            }
            catch (Exception e)
            {
                Model.SystemLog.addEntry("Exception!");
                Model.SystemLog.addEntry(e.Message);
                Model.SystemLog.addEntry(e.StackTrace);
            }
#endif
        }
        public static bool doneLoading = false;

        public static string currentLoadState = "";
        
        //TODO: Remove old incoming connections when they're dead
        static void acceptLoop()
        {
            while (!disposed)
            {
                System.Net.Sockets.TcpClient u = listener.AcceptTcpClient();
                Model.IncomingConnection c = new Model.ReliableIncomingConnection(u);
                theCore.addIncomingConnection(c);
            }
        }
    }
}
