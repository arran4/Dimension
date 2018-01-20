﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    public class PeerManager
    {
        public Peer[] allPeers
        {
            get
            {
                lock (peers)
                    return ((new List<Peer>(peers.Values)).ToArray());
            }
        }
        public Peer[] allPeersInCircle(ulong id)
        {
            System.Security.Cryptography.SHA512Managed sha = new System.Security.Cryptography.SHA512Managed();
            ulong lanHash = BitConverter.ToUInt64(sha.ComputeHash(Encoding.UTF8.GetBytes("LAN".ToLower())), 0);

            List<Peer> output = new List<Peer>();
            foreach (Peer p in allPeers)
                if (p.circles.Contains(id) && !p.quit)
                    if (id == lanHash)
                    {
                        if (p.isLocal)
                            output.Add(p);
                    }else
                        output.Add(p);
            return output.ToArray();
        }
        public void doPeerRemoved(Peer p)
        {
            foreach(ulong u in p.circles)
                peerRemoved?.Invoke(p, u, true);
        }
        public bool havePeerWithAddress(string[] i, System.Net.IPAddress e)
        {
            if (Program.theCore.internalIPs.Contains(e))
                return true;
            if (e.ToString() == Program.bootstrap.publicControlEndPoint.Address.ToString())
            {
                //they're local
                foreach (Peer p in allPeers)
                    foreach(string inte in i)
                    if (inte == i.ToString())
                        return true;
            }
            else
            {
                foreach (Peer p in allPeers)
                    if (p.publicAddress.ToString() == e.ToString())
                        return true;

            }
            return false;
        }
        public delegate void PeerRenameEvent(string oldName, Peer p);
        public delegate void PeerUpdateEvent(Peer p);
        public delegate void PeerChannelUpdateEvent(Peer p, ulong channel, bool update);
        public event PeerRenameEvent peerRenamed;
        public event PeerChannelUpdateEvent peerAdded;
        public event PeerUpdateEvent peerUpdated;
        public event PeerChannelUpdateEvent peerRemoved;
        Dictionary<ulong, Peer> peers = new Dictionary<ulong, Peer>();
        public bool parseMiniHello(Commands.MiniHello h, System.Net.IPEndPoint sender)
        {
            lock (peers)
            {
                if (peers.ContainsKey(h.id))
                {
                    peers[h.id].quit = false;
                    peers[h.id].lastContact = DateTime.Now;
                }
                else
                {
                    return true;
                }
                return false;
            }
        }
        public void parseHello(Commands.HelloCommand h, System.Net.IPEndPoint sender)
        {
            bool wasQuit = false;
            List<ulong> channels = new List<ulong>();
            List<ulong> oldChannels = new List<ulong>();
            string oldName = "";
            bool renamed = false;
            bool updated = false;
            bool added = false;
            lock (peers)
            {
                if (peers.ContainsKey(h.id))
                {
                    wasQuit = peers[h.id].quit;
                    try
                    {
                        System.Net.IPAddress[] ips = new System.Net.IPAddress[h.internalIPs.Length];
                        for (int i = 0; i < ips.Length; i++)
                            ips[i] = System.Net.IPAddress.Parse(h.internalIPs[i]);
                        peers[h.id].internalAddress = ips;
                    }
                    catch
                    {
                        //whatever
                        peers[h.id].internalAddress = new System.Net.IPAddress[] { System.Net.IPAddress.Loopback };
                    }
                    if (peers[h.id].quit)
                        added = true;
                    peers[h.id].quit = false;
                    peers[h.id].useUDT = h.useUDT;
                    peers[h.id].actualEndpoint = sender;
                    if (h.externalIP != null)
                        peers[h.id].publicAddress = System.Net.IPAddress.Parse(h.externalIP);
                    if (peers[h.id].share != h.myShare)
                    {
                        peers[h.id].share = h.myShare;
                        updated = true;
                    }
                    string s1 = "";
                    string s2 = "";
                    foreach (int i in peers[h.id].circles)
                        s1 += i.ToString() + ", ";
                    foreach (int i in h.myCircles)
                        s2 += i.ToString() + ", ";
                    if (s1 != s2)
                    {
                        oldChannels.AddRange(peers[h.id].circles);
                        peers[h.id].circles = h.myCircles;
                        added = true;
                    }
                }
                else
                {
                    peers[h.id] = new Peer();
                    peers[h.id].id = h.id;
                    peers[h.id].actualEndpoint = sender;
                    if (h.externalIP != null)
                        peers[h.id].publicAddress = System.Net.IPAddress.Parse(h.externalIP);
                    peers[h.id].username = h.username;
                    peers[h.id].circles = h.myCircles;
                    added = true;
                }
                if (peers[h.id].description != h.description)
                {
                    peers[h.id].description = h.description;
                    updated = true;
                }
                peers[h.id].behindDoubleNAT = h.behindDoubleNAT;
                peers[h.id].externalControlPort = h.externalControlPort;
                peers[h.id].externalDataPort = h.externalDataPort;
                peers[h.id].localControlPort = h.internalControlPort;
                peers[h.id].localDataPort = h.internalDataPort;
                peers[h.id].localUDTPort = h.internalUdtPort;
                if (peers[h.id].username != h.username)
                {
                    oldName = peers[h.id].username;
                    peers[h.id].username = h.username;
                    renamed = true;
                }
                if (peers[h.id].afk != h.afk)
                {
                    peers[h.id].afk = h.afk;
                    updated = true;
                }
                peers[h.id].quit = false;
                peers[h.id].peerCount = h.peerCount;
                peers[h.id].lastContact = DateTime.Now;
                peers[h.id].buildNumber = h.buildNumber;
                channels.AddRange(peers[h.id].circles);
            }
            if (updated) peerUpdated?.Invoke(peers[h.id]);
            if (renamed) peerRenamed?.Invoke(oldName, peers[h.id]);
            if (added)
            {

                System.Security.Cryptography.SHA512Managed sha = new System.Security.Cryptography.SHA512Managed();
                ulong lanHash = BitConverter.ToUInt64(sha.ComputeHash(Encoding.UTF8.GetBytes("LAN".ToLower())), 0);

                foreach (ulong u in channels)
                    if (u == lanHash)
                    {
                        if (!oldChannels.Contains(u) && peers[h.id].isLocal)
                            peerAdded?.Invoke(peers[h.id], u, true);
                    }
                    else
                    {
                        if (!oldChannels.Contains(u))
                            peerAdded?.Invoke(peers[h.id], u, true);
                    }
                
                foreach (ulong u in oldChannels)
                    if (u == lanHash)
                    {
                        if (!channels.Contains(u) && peers[h.id].isLocal)
                            peerRemoved?.Invoke(peers[h.id], u, !wasQuit);
                    }
                    else
                    {
                        if (!channels.Contains(u))
                            peerRemoved?.Invoke(peers[h.id], u, !wasQuit);
                    }
                
            }
        }
    }
}
