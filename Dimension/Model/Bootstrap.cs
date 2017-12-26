﻿using System;
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
        //FIXME: Gracefully handle LAN environments where STUN and UPnP might not be available
        //FIXME: If a port is already taken on the router but free on this machine, UPnP will crash
        //FIXME: If the unreliable port is already taken, or unavailable on the router, or outside the range of valid ports -- it will crash

        public bool active = false;
        
        public Bootstrap()
        {
        }
        public void Dispose()
        {
            if (Program.settings.getBool("Use UPnP", true) && active)
                unMapPorts().Wait();
        }
        async Task unMapPorts()
        {
            try
            {
                NatDiscoverer d = new NatDiscoverer();
                NatDevice device = await d.DiscoverDeviceAsync();
                foreach (Mapping z in await device.GetAllMappingsAsync())
                    if (z.Description == Environment.MachineName + " Dimension Mapping")
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

                Mapping m = new Mapping(Protocol.Udp, internalPort, externalPort, Environment.MachineName + " Dimension Mapping");
                await device.CreatePortMapAsync(m);

                Mapping m2 = new Mapping(Protocol.Udp, internalPort+1, externalPort+1, Environment.MachineName + " Dimension Mapping");
                await device.CreatePortMapAsync(m2);
            }
            catch
            {
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
        public Udt.Socket udtSocket;
        public IPEndPoint publicDataEndPoint;
        public IPEndPoint publicControlEndPoint;
        public async Task launch()
        {
            Program.currentLoadState = "Binding UDP sockets";
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            
            try
            {
                socket.Bind(new IPEndPoint(IPAddress.Any, Program.settings.getInt("Default Data Port", 0)));
            }
            catch
            {
                socket.Bind(new IPEndPoint(IPAddress.Any, 0));
            }

            unreliableClient = new UdpClient(Program.settings.getInt("Default Control Port", 0));

            Program.currentLoadState = "STUNning NAT";
            STUN_Result result = STUN_Client.Query("stun.l.google.com", 19302, unreliableClient.Client);
            if (result.NetType == STUN_NetType.UdpBlocked)
            {
                Program.currentLoadState = "STUN failed. Running in LAN mode.";
                active = false;
                return;
            }
            publicControlEndPoint = result.PublicEndPoint;

            STUN_Result result2 = STUN_Client.Query("stun.l.google.com", 19302, socket);
            publicDataEndPoint = result2.PublicEndPoint;

            if (Program.settings.getBool("Use UPnP", true))
            {
                Program.currentLoadState = "Mapping UPnP ports.";
                await mapPorts(((IPEndPoint)unreliableClient.Client.LocalEndPoint).Port, publicControlEndPoint.Port);
                await mapPorts(((IPEndPoint)socket.LocalEndPoint).Port, publicDataEndPoint.Port);
            }

            Program.currentLoadState = "Setting up data socket.";
            udtSocket = new Udt.Socket(AddressFamily.InterNetwork, SocketType.Stream);
            udtSocket.Bind(socket);
        }
    }

}
