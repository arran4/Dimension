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
    class Bootstrap : IDisposable
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
        async Task unMapPorts()
        {
            SystemLog.addEntry("Cleaning up UPnP mappings...");
            try
            {
                NatDiscoverer d = new NatDiscoverer();
                NatDevice device = await d.DiscoverDeviceAsync();
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
        async Task mapPorts(int internalPort, int externalPort, bool tcp)
        {

            try
            {
                NatDiscoverer d = new NatDiscoverer();
                NatDevice device = await d.DiscoverDeviceAsync();
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
            //TODO: Gracefully handle URLs that don't exist
            WebRequest r = WebRequest.Create(address + "?port=" + publicControlEndPoint.Port.ToString());
            string response = (new StreamReader(r.GetResponse().GetResponseStream())).ReadToEnd();

            string[] split = response.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            IPEndPoint[] output = new IPEndPoint[split.Length];
            for (int i = 0; i < split.Length; i++)
                output[i] = new IPEndPoint(IPAddress.Parse(split[i].Split(' ')[0]), int.Parse(split[i].Split(' ')[1]));

            return output;
        }
        public UdpClient unreliableClient;
        public TcpListener listener;
        public IPEndPoint publicDataEndPoint;
        public IPEndPoint publicControlEndPoint;
        public int internalControlPort;
        public int internalDataPort;
        bool LANMode = false;
        public async Task launch()
        {
            SystemLog.addEntry("Beginning network setup...");
            Program.currentLoadState = "Deleting old UPnP mappings...";
            if (Program.settings.getBool("Use UPnP", true))
            {

                bool done = false;
                System.Threading.Semaphore s = new System.Threading.Semaphore(0, 1);
                System.Threading.Thread t = new System.Threading.Thread(async delegate ()
                {
                    await unMapPorts();
                    s.Release();
                    done = true;
                });
                t.IsBackground = true;
                t.Name = "UPnP test thread";
                t.Start();

                if (!done)
                    s.WaitOne(10000);
                if (!done)
                    Program.settings.setBool("Use UPnP", false);
            }
            Program.currentLoadState = "Binding UDP sockets.";
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            listener = new TcpListener(IPAddress.Any,Program.settings.getInt("Default Data Port", 0));

            listener.Start();
            unreliableClient = new UdpClient(Program.settings.getInt("Default Control Port", NetConstants.controlPort));
            internalControlPort = ((IPEndPoint)unreliableClient.Client.LocalEndPoint).Port;
            SystemLog.addEntry("Successfully bound to UDP control port " + internalControlPort);

            Program.currentLoadState = "STUNning NAT";
            try
            {
                string stunUrl = "stun.l.google.com";
                STUN_Result result = STUN_Client.Query(stunUrl, 19302, unreliableClient.Client);
                SystemLog.addEntry("Attempting to STUN control port to " + stunUrl + ".");
                if (result.NetType == STUN_NetType.UdpBlocked)
                {
                    SystemLog.addEntry("STUN failed. Assuming network is LAN-only.");
                    Program.currentLoadState = "STUN failed. Running in LAN mode.";
                    UPnPActive = false;
                    return;
                }
                Random r = new Random();
                SystemLog.addEntry("STUN successful. External control endpoint: " + result.PublicEndPoint.ToString());
                publicControlEndPoint = new IPEndPoint(result.PublicEndPoint.Address, r.Next(short.MaxValue-1000)+1000);
                publicDataEndPoint = new  IPEndPoint(result.PublicEndPoint.Address, r.Next(short.MaxValue - 1000) + 1000);
            }
            catch (Exception) //STUN can throw generic exceptions :(
            {
                SystemLog.addEntry("Failed to STUN. Working in LAN mode.");
                publicControlEndPoint = null;
                publicDataEndPoint = null;
                //Stun failed, offline mode
                LANMode = true;
            }

            if (Program.settings.getBool("Use UPnP", true) && !LANMode && UPnPActive)
            {
                SystemLog.addEntry("UPnP enabled. Attempting to map UPnP ports...");
                Program.currentLoadState = "Mapping UPnP ports.";
                SystemLog.addEntry("Creating control UPnP mapping...");
                await mapPorts(((IPEndPoint)unreliableClient.Client.LocalEndPoint).Port, publicControlEndPoint.Port, false);
                SystemLog.addEntry("Creating data UPnP mapping...");
                await mapPorts(((IPEndPoint)listener.Server.LocalEndPoint).Port, publicDataEndPoint.Port, true);
            }

            internalDataPort = ((IPEndPoint)listener.Server.LocalEndPoint).Port;
            SystemLog.addEntry("Network setup complete.");
            UPnPActive = true;
        }
    }

}
