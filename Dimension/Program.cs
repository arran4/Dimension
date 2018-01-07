using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dimension
{
    static class Program
    {
        public static bool comicSansOnly
        {
            get
            {
                return settings.getString("Username", "Username").ToLower().Trim() == "zardoz";
            }
        }
        public static System.Drawing.Font getFont()
        {
            if (comicSansOnly || settings.getString("Font", "Lucida Console") == "Comic Sans MS")
                return new System.Drawing.Font("Comic Sans MS", 14);
            else
                return new System.Drawing.Font(settings.getString("Font", "Lucida Console"), 8.25f);
        }
        public const int buildNumber = 44;
        public static Model.GlobalSpeedLimiter speedLimiter;
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
            speedLimiter.Dispose();
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

            speedLimiter = new Model.GlobalSpeedLimiter();
            string username = settings.getString("Username", Environment.MachineName);
            settings.setString("Username", username);

            Model.SystemLog.addEntry("Setting up NAT...");
            bootstrap = new Model.Bootstrap();
            bootstrap.launch().Wait();
            listener = bootstrap.listener;
            udp = bootstrap.unreliableClient;
            System.Threading.Thread t = new System.Threading.Thread(acceptLoop);
            t.IsBackground = true;
            t.Name = "UDT Accept Loop";
            t.Start();
            fileListDatabase = new Model.FileListDatabase();

            Model.SystemLog.addEntry("Creating File List Manager...");
            fileList = new Model.FileList();

            Model.SystemLog.addEntry("Starting a file list update...");
            fileList.startUpdate(false);

            Model.SystemLog.addEntry("Creating a serializer...");
            serializer = new Model.Serializer();

            Model.SystemLog.addEntry("Creating the client core...");
            theCore = new Model.Core();

            Model.SystemLog.addEntry("Starting Kademlia launching...");
            t = new System.Threading.Thread(delegate ()
            {
                kademlia = new Model.Kademlia();
                kademlia.initialize();
            });
            t.IsBackground = true;
            t.Name = "Kademlia Init Thread";
            t.Start();

            Model.SystemLog.addEntry("Done loading!");
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
