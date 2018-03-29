using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension
{
    public class App
    {
        public const int buildNumber = 109;
        public static Model.GlobalSpeedLimiter speedLimiter;
        public static System.Windows.Forms.Form mainForm;
        public static Model.ByteCounter globalUpCounter = new Model.ByteCounter();
        public static Model.ByteCounter globalDownCounter = new Model.ByteCounter();
        static object updateRequestLock = new object();
        static bool checking = false;

        static bool updateBegin = false;
        public static Model.Kademlia kademlia;
        public static Model.Core theCore;
        public static System.Net.Sockets.UdpClient udp;
        public static System.Net.Sockets.UdpClient udp2;
        public static System.Net.Sockets.TcpListener listener;
        public static Model.Bootstrap bootstrap;
        public static Model.FileListDatabase fileListDatabase;
        public static Model.Settings settings;
        public static Model.FileList fileList;
        public static Model.Serializer serializer;
        static bool updateDeclined = false;
        public static bool isMono
        {
            get
            {
                return Type.GetType("Mono.Runtime") != null;
            }
        }
        public static bool comicSansOnly
        {
            get
            {
                return bootstrap.behindDoubleNAT;
            }
        }
        public static System.Drawing.Font getFont()
        {
            if (comicSansOnly || settings.getString("Font", "Lucida Console") == "Comic Sans MS")
                return new System.Drawing.Font("Comic Sans MS", 14);
            else
                return new System.Drawing.Font(settings.getString("Font", "Lucida Console"), 8.25f);
        }
        public static void downloadUpdates()
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "Updater.exe";
            p.StartInfo.Arguments = buildNumber.ToString();
            p.Start();
        }
        public static bool checkForUpdates()
        {
            if (checking)
                return false;
            checking = true;
            if (App.settings.getBool("Update Without Prompting", false))
            {
                if (Updater.Program.needsUpdate(buildNumber))
                {
                    if (updateBegin)
                        return false;
                    updateBegin = true;
                    downloadUpdates();
                    System.Windows.Forms.Application.Exit();
                    checking = false;
                    return true;
                }
            }
            try
            {
                lock (updateRequestLock)
                {
                    if (!updateDeclined)
                    {
                        if (Updater.Program.needsUpdate(buildNumber))
                        {
                            if (System.Windows.Forms.MessageBox.Show("An update is available. Would you like to download it?", "Dimension Update", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                            {
                                if (updateBegin)
                                    return false;
                                updateBegin = true;
                                if (isMono)
                                {
                                    try
                                    {
                                        System.Diagnostics.Process.Start(Updater.Program.downloadPath());
                                    }
                                    catch (System.ComponentModel.Win32Exception)
                                    {
                                        //Wine sometimes throws these, doesn't mean anything
                                    }
                                }
                                else
                                {
                                    downloadUpdates();
                                }
                                System.Windows.Forms.Application.Exit();
                                checking = false;
                                return true;
                            }
                            else
                                updateDeclined = true;
                        }
                    }
                }
            }
            catch
            {
                //whatever
            }
            checking = false;
            return false;
        }
        static bool disposed = false;
        public static void doCleanup()
        {
            disposed = true;
            if (!isMono)
            {
                kademlia.Dispose();
            }
            fileList.Dispose();
            fileListDatabase.close();
            bootstrap.Dispose();
            speedLimiter.Dispose();
            Model.SystemLog.writer.Close();

            theCore.Dispose();

        }
        public static void udpSend(byte[] b, int length, System.Net.IPEndPoint target)
        {
            udpSend(b, target);
        }
        public static void udpSend(byte[] b, System.Net.IPEndPoint target)
        {
            try
            {
                lock (theCore.outgoingTraffic)
                {
                    string t = App.serializer.getType(b);
                    if (!theCore.outgoingTraffic.ContainsKey(t))
                        theCore.outgoingTraffic[t] = 0;
                    theCore.outgoingTraffic[t] += (ulong)b.Length;
                    App.globalUpCounter.addBytes(b.Length);
                }
            }
            catch
            {
                //whatever
            }
            try
            {
                udp.Send(b, b.Length, target);
            }
            catch
            {
                //probably no path
            }
        }
        public static void doLoad()
        {
#if DEBUG
#else
            try
            {
#endif
            Model.SystemLog.addEntry("---------------");
            Model.SystemLog.addEntry("");
            Model.SystemLog.addEntry("Startup at " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString());


            speedLimiter = new Model.GlobalSpeedLimiter();
            string username = settings.getString("Username", Environment.MachineName);
            settings.setString("Username", username);


            try
            {
                udp2 = new System.Net.Sockets.UdpClient(Dimension.Model.NetConstants.controlPort);
            }
            catch
            {
            }

            Model.SystemLog.addEntry("Setting up NAT...");
            bootstrap = new Model.Bootstrap();
            try
            {
                bootstrap.launch().Wait();
            }
            catch (System.TypeLoadException)
            {
                System.Windows.Forms.MessageBox.Show("Error! It looks like you don't have .NET 4.5 x86 installed.");
                System.Windows.Forms.Application.Exit();
                return;
            }
            listener = bootstrap.listener;
            udp = bootstrap.unreliableClient;
            System.Threading.Thread t = new System.Threading.Thread(acceptLoop);
            t.IsBackground = true;
            t.Name = "UDT Accept Loop";
            t.Start();
            fileListDatabase = new Model.FileListDatabase();

            Model.SystemLog.addEntry("Creating File List Manager...");
            fileList = new Model.FileList();


            Model.SystemLog.addEntry("Creating a serializer...");
            serializer = new Model.Serializer();

            Model.SystemLog.addEntry("Creating the client core...");
            theCore = new Model.Core();

            Model.SystemLog.addEntry("Starting Kademlia launching...");
            kademlia = new Model.Kademlia();
            if (!isMono)
            {
                t = new System.Threading.Thread(delegate ()
                {
                    kademlia.initialize();
                });
                t.IsBackground = true;
                t.Name = "Kademlia Init Thread";
                t.Start();
            }
            Model.SystemLog.addEntry("Starting a file list update...");
            fileList.startUpdate(false);

            Model.SystemLog.addEntry("Saving settings...");
            App.settings.save();

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


        public delegate void PrivateChatReceived(Model.Commands.PrivateChatCommand c, Model.Peer z);
        public static event PrivateChatReceived privateChatReceived;
        public static void doPrivateChatReceived(Model.Commands.PrivateChatCommand c, Model.Peer z)
        {
            if (privateChatReceived != null)
                privateChatReceived(c, z);
        }
        public static event Action flash;
        public static void doFlash()
        {
            if (flash != null)
                flash();
        }
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
