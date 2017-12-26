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
    class Bootstrap : IDisposable
    {
        //TODO: Add capability to simply disable UPnP
        //TODO: Gracefully handle web request failing if the bootstrap is down
        //TODO: Have an accumulated list of nodes for direct access if bootstrap is down
        //TODO: Gracefully handle invalid bootstrap response (error codes, or invalid IP/port list, or non-integer ports
        //TODO: Gracefully handle a node that provides an invalid host/port number through Chord
        //FIXME: Gracefully handle LAN environments where STUN and UPnP might not be available
        //FIXME: If a port is already taken on the router but free on this machine, UPnP will crash
        //FIXME: If a port is free for UDP locally but not free on TCP, NChord will crash
        public bool active = false;

        public int preferredPort = -1;
        public bool useUPnP = true;
        public Bootstrap()
        {
        }
        public void Dispose()
        {
            if (useUPnP)
                unMapPorts().Wait();
        }
        async Task unMapPorts()
        {
            try
            {
                NatDiscoverer d = new NatDiscoverer();
                NatDevice device = await d.DiscoverDeviceAsync();
                foreach (Mapping z in await device.GetAllMappingsAsync())
                    if (z.Description == Environment.MachineName + " Dimension Chord Mapping")
                        await device.DeletePortMapAsync(z);
            }
            catch
            {
                //UPnP probably not supported
            }
        }
        async Task mapPorts(int internalPort, int externalPort)
        {
            await unMapPorts();

            try
            {
                NatDiscoverer d = new NatDiscoverer();
                NatDevice device = await d.DiscoverDeviceAsync();

                Mapping m = new Mapping(Protocol.Udp, internalPort, externalPort, Environment.MachineName + " Dimension Chord Mapping");
                await device.CreatePortMapAsync(m);
            }
            catch
            {
                //UPnP probably not supported
            }
        }
        public IPEndPoint[] join(string address)
        {
            WebRequest r = WebRequest.Create(address + "?port=" + publicEndPoint.Port.ToString());
            string response = (new StreamReader(r.GetResponse().GetResponseStream())).ReadToEnd();

            string[] split = response.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            IPEndPoint[] output = new IPEndPoint[split.Length];
            for (int i = 0; i < split.Length; i++)
                output[i] = new IPEndPoint(IPAddress.Parse(split[i].Split(' ')[0]), int.Parse(split[i].Split(' ')[1]));

            return output;
        }
        public Udt.Socket udtSocket;
        public IPEndPoint publicEndPoint;
        public async Task launch()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            if (preferredPort != -1)
            {
                try
                {
                    socket.Bind(new IPEndPoint(IPAddress.Any, preferredPort));
                }
                catch
                {
                    socket.Bind(new IPEndPoint(IPAddress.Any, 0));
                }
            }
            else
                socket.Bind(new IPEndPoint(IPAddress.Any, 0));

            STUN_Result result = STUN_Client.Query("stun.l.google.com", 19302, socket);
            if (result.NetType == STUN_NetType.UdpBlocked)
            {
                active = false;
                return;
            }
            int internalPort = ((IPEndPoint)socket.LocalEndPoint).Port;
            publicEndPoint = result.PublicEndPoint;

            if(useUPnP)
                await mapPorts(internalPort, publicEndPoint.Port);

            udtSocket = new Udt.Socket(AddressFamily.InterNetwork, SocketType.Stream);
            udtSocket.Bind(socket);
        }
    }

}
