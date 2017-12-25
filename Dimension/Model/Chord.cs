using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using LumiSoft.Net.STUN.Client;
using NChordLib;
using System.IO;
using Open.Nat;

namespace Dimension.Model
{
    class Chord :IDisposable
    {
        public bool active = false;
        public Chord()
        {
        }
        public void Dispose()
        {
            unMapPorts().Wait();
        }
        async Task unMapPorts()
        {
            NatDiscoverer d = new NatDiscoverer();
            NatDevice device = await d.DiscoverDeviceAsync();
            foreach (Mapping z in await device.GetAllMappingsAsync())
                if (z.Description == Environment.MachineName + " Dimension Chord Mapping")
                    await device.DeletePortMapAsync(z);
        }
        async Task mapPorts(int port)
        {
            NatDiscoverer d = new NatDiscoverer();
            NatDevice device = await d.DiscoverDeviceAsync();

            foreach (Mapping z in await device.GetAllMappingsAsync())
                if (z.Description == Environment.MachineName + " Dimension Chord Mapping")
                    await device.DeletePortMapAsync(z);
            Mapping m = new Mapping(Protocol.Tcp, port, port, Environment.MachineName + " Dimension Chord Mapping");
            await device.CreatePortMapAsync(m);
        }
        //FIXME: Gracefully handle LAN environments where STUN and UPnP might not be available
        //FIXME: If a port is already taken on the router but free on this machine, UPnP will crash
        //FIXME: If a port is free for UDP locally but not free on TCP, NChord will crash
        public async Task launch()
        {

            UdpClient udp = new UdpClient(0);   //pick a port, any port
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(new IPEndPoint(IPAddress.Any, 0));

            STUN_Result result = STUN_Client.Query("stun.l.google.com", 19302, socket);
            if (result.NetType == STUN_NetType.UdpBlocked)
            {
                active = false;
                return;
            }
            IPEndPoint publicEndPoint = result.PublicEndPoint;

            await mapPorts(publicEndPoint.Port);
            WebRequest r = WebRequest.Create("http://www.9thcircle.net/Projects/Dimension/bootstrap.php?port=" + publicEndPoint.Port.ToString());
            string response = (new StreamReader(r.GetResponse().GetResponseStream())).ReadToEnd();

            ChordServer.LocalNode = new ChordNode(System.Net.Dns.GetHostName(), publicEndPoint.Port);
            ChordInstance instance = ChordServer.GetInstance(ChordServer.LocalNode);

            string[] split = response.Split('\n');
            for (int i = 0; i < split.Length; i++)
            {
                IPAddress ip;
                try
                {
                    ip = IPAddress.Parse(split[i].Split(' ')[0]);
                }
                catch (FormatException)
                {
                    //Invalid IP address format from the server, ignore it
                    continue;
                }
                int port = int.Parse(split[i].Split(' ')[1]);

                if (ip.ToString() != publicEndPoint.Address.ToString())
                {
                    try
                    {
                        if (instance.Join(null, ChordServer.LocalNode.Host, ChordServer.LocalNode.PortNumber))
                            break;
                    }
                    catch (SocketException)
                    {
                        //Couldn't connect, move on
                    }
                }
            }
        }
        }
}
