using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using LumiSoft.Net.STUN.Client;
using System.IO;
using Open.Nat;

namespace Dimension.Model
{
    class Bootstrap : System.Diagnostics.TraceListener, IDisposable
    {
        //TODO: Add capability to simply disable UPnP
        //TODO: Gracefully handle web request failing if the bootstrap is down
        //TODO: Have an accumulated list of nodes for direct access if bootstrap is down
        //TODO: Gracefully handle invalid bootstrap response (error codes, or invalid IP/port list, or non-integer ports
        //FIXME: Gracefully handle LAN environments where STUN and UPnP might not be available
        //FIXME: If a port is already taken on the router but free on this machine, UPnP will crash
        //FIXME: If the unreliable port is already taken, or unavailable on the router, or outside the range of valid ports -- it will crash

        public bool UPnPActive = true;
        
        public Bootstrap()
        {
        }
        public void Dispose()
        {
            if (Program.settings.getBool("Use UPnP", true) && UPnPActive && !LANMode)
                unMapPorts().Wait();
        }

        public override void WriteLine(string message)
        {
            SystemLog.addEntry(message);
        }

        public override void Write(string message)
        {
            SystemLog.addEntry(message);
        }
        static NatDevice device = null;

        void deviceFound(object sender, DeviceEventArgs e)
        {
            device = e.Device;
            upnpSem.Release();
        }
        void initUPnP()
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    NatUtility.Initialize();
                    NatUtility.DeviceFound += deviceFound;
                    NatUtility.StartDiscovery();
                    upnpSem.WaitOne();
                    NatUtility.StopDiscovery();
                    break;
                }
                catch (Exception e)
                {
                    System.Threading.Thread.Sleep(1000);
                    if (i < 5)
                        SystemLog.addEntry("Failed to boot up UPnP. Exception: " + e.Message + ". Trying again...");
                    else
                    {
                        SystemLog.addEntry("Giving up on UPnP and running a STUN instead...");
                        UPnPActive = false;
                        return;
                    }
                }
            }
        }
        System.Threading.Semaphore upnpSem = new System.Threading.Semaphore(0, 1);
        async Task unMapPorts()
        {
            SystemLog.addEntry("Cleaning up UPnP mappings...");
            try
            {
                if (device == null)
                {
                    initUPnP();
                }
                externalIPFromUPnP = await device.GetExternalIPAsync();
                foreach (Mapping z in await device.GetAllMappingsAsync())
                    if (z.Description == Environment.MachineName + " Dimension Mapping")
                    {
                        await device.DeletePortMapAsync(z);
                        SystemLog.addEntry("Successfully deleted UPnP mapping " + z.Description);
                        UPnPActive = true;
                    }
            }
            catch
            {
                UPnPActive = false;
                SystemLog.addEntry("Failed to delete UPnP mapping.");
                //UPnP probably not supported
            }
        }
        IPAddress externalIPFromUPnP;
        async Task mapPorts(int internalPort, int externalPort, bool tcp)
        {

            try
            {
                if (device == null)
                {
                    initUPnP();
                }
                externalIPFromUPnP = await device.GetExternalIPAsync();
                SystemLog.addEntry("Successfully found UPnP device "  +await device.GetExternalIPAsync());

                Mapping m = new Mapping(tcp ? Protocol.Tcp : Protocol.Udp, internalPort, externalPort, Environment.MachineName + " Dimension Mapping");
                await device.CreatePortMapAsync(m);
                SystemLog.addEntry("Successfully created UPnP mapping from port " + internalPort.ToString() + " to " +externalPort.ToString());
                UPnPActive = true;
            }
            catch
            {
                UPnPActive = false;
                //UPnP probably not supported
            }
        }
        public IPEndPoint[] join(string address)
        {
            string response;
            try
            {
                WebRequest r = WebRequest.Create(address + "?port=" + publicControlEndPoint.Port.ToString());
                response = (new StreamReader(r.GetResponse().GetResponseStream())).ReadToEnd();
            }
            catch(Exception e)
            {
                return new IPEndPoint[] { };
            }
            string[] split = response.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> nonDuplicates = new List<string>();
            foreach (string s in split)
                if (!nonDuplicates.Contains(s))
                    nonDuplicates.Add(s);
            split = nonDuplicates.ToArray();
            IPEndPoint[] output = new IPEndPoint[split.Length];
            for (int i = 0; i < split.Length; i++)
                try
                {
                    output[i] = new IPEndPoint(IPAddress.Parse(split[i].Split(' ')[0]), int.Parse(split[i].Split(' ')[1]));
                }
                catch
                {
                    output[i] = null;
                }

            return output;
        }
        public UdpClient unreliableClient;
        public TcpListener listener;
        public IPEndPoint publicDataEndPoint;
        public IPEndPoint publicControlEndPoint;
        public int publicDHTPort;
        public int internalControlPort;
        public int internalDataPort;
        public int internalDHTPort;
        bool LANMode = false;
        public bool behindDoubleNAT = false;
        public async Task launch()
        {
            SystemLog.addEntry("Beginning network setup...");
            SystemLog.addEntry("Deleting old UPnP mappings...");
            if (Program.settings.getBool("Use UPnP", true))
            {

                bool done = false;
                System.Threading.Semaphore s = new System.Threading.Semaphore(0, 1);
                System.Threading.Thread t = new System.Threading.Thread(async delegate ()
                {
                    await unMapPorts();
                    done = true;
                    s.Release();
                });
                t.IsBackground = true;
                t.Name = "UPnP test thread";
                t.Start();

                if (!done)
                    s.WaitOne(10000);
                if (!done)
                {
                    SystemLog.addEntry("Failed to find UPnP router in a timely fashion. Disabling UPnP...");
                    UPnPActive = false;
                }
            }
            SystemLog.addEntry("Binding UDP sockets.");
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);


            listener = new TcpListener(IPAddress.Any,Program.settings.getInt("Default Data Port", 0));

            listener.Start();
            internalDataPort = ((IPEndPoint)listener.Server.LocalEndPoint).Port;
            SystemLog.addEntry("Binding to TCP port " + internalDataPort.ToString());
            int control = Program.settings.getInt("Default Control Port", 0);
            if (control == Dimension.Model.NetConstants.controlPort)
                control = 0;
            unreliableClient = new UdpClient(control);
            internalControlPort = ((IPEndPoint)unreliableClient.Client.LocalEndPoint).Port;
            if (control == 0)
                Program.settings.setInt("Default Control Port", internalControlPort);
            SystemLog.addEntry("Successfully bound to UDP control port " + internalControlPort);

            publicControlEndPoint = (IPEndPoint)unreliableClient.Client.LocalEndPoint;
            publicDataEndPoint = (IPEndPoint)listener.Server.LocalEndPoint;
            
            tryAgain:

            if (Program.settings.getBool("Use UPnP", true) == false || LANMode || !UPnPActive || behindDoubleNAT)
            {
                SystemLog.addEntry("STUNning NAT");
                try
                {
                    string stunUrl = "stun.l.google.com";
                    STUN_Result result = STUN_Client.Query(stunUrl, 19302, unreliableClient.Client);
                    SystemLog.addEntry("Attempting to STUN control port to " + stunUrl + ".");

                    if (result.NetType == STUN_NetType.UdpBlocked)
                    {
                        SystemLog.addEntry("STUN failed. Assuming network is LAN-only.");
                        LANMode = true;
                        UPnPActive = false;
                    }
                    else
                    {
                        publicControlEndPoint = new IPEndPoint(result.PublicEndPoint.Address, result.PublicEndPoint.Port);
                        publicDataEndPoint = new IPEndPoint(result.PublicEndPoint.Address, internalDataPort);
                        SystemLog.addEntry("STUN successful. External control endpoint: " + result.PublicEndPoint.ToString());
                        SystemLog.addEntry("External data endpoint: " + publicDataEndPoint.ToString());

                    }
                }
                catch (Exception) //STUN can throw generic exceptions :(
                {
                    SystemLog.addEntry("Failed to STUN. Working in LAN mode.");
                    //Stun failed, offline mode
                    LANMode = true;
                }
            }

           

            Random r = new Random();
            internalDHTPort = Program.settings.getInt("Default DHT Port", 0);
            if (internalDHTPort == 0)
                internalDHTPort = r.Next(short.MaxValue - 1000) + 1000;
            publicDHTPort = internalDHTPort;
            if (Program.settings.getBool("Use UPnP", true) && !LANMode && UPnPActive && !behindDoubleNAT)
            {
                SystemLog.addEntry("UPnP enabled. Attempting to map UPnP ports...");

                publicControlEndPoint = new IPEndPoint(publicControlEndPoint.Address, r.Next(short.MaxValue - 1000) + 1000);
                publicDataEndPoint = new IPEndPoint(publicControlEndPoint.Address, r.Next(short.MaxValue - 1000) + 1000);
                publicDHTPort = r.Next(short.MaxValue - 1000) + 1000;

                SystemLog.addEntry("Creating control UPnP mapping (random external port)...");
                await mapPorts(((IPEndPoint)unreliableClient.Client.LocalEndPoint).Port, publicControlEndPoint.Port, false);
                SystemLog.addEntry("Creating data UPnP mapping (random external port)...");
                await mapPorts(((IPEndPoint)listener.Server.LocalEndPoint).Port, publicDataEndPoint.Port, true);
                SystemLog.addEntry("Creating DHT UPnP mapping (random external port)...");
                await mapPorts(internalDHTPort, publicDHTPort, false);

                publicControlEndPoint = new IPEndPoint(externalIPFromUPnP, publicControlEndPoint.Port);
                publicDataEndPoint = new IPEndPoint(externalIPFromUPnP, publicDataEndPoint.Port);

                if (externalIPFromUPnP.ToString().StartsWith("10.") || externalIPFromUPnP.ToString().StartsWith("192."))
                {
                    behindDoubleNAT = true;
                    SystemLog.addEntry("WARNING! Your router provided a local IP address as the external endpoint.");
                    SystemLog.addEntry("This probably means you're running more than one router in a row (double NAT).");
                    SystemLog.addEntry("Dimension is going to disable UPnP and try STUNning again to get through this.");
                    SystemLog.addEntry("If this is your home network, please talk to a network engineer -- having UPnP with double NAT is a very bad idea.");
                    goto tryAgain;
                }
            }
            SystemLog.addEntry("Network setup complete.");
        }
    }

}
