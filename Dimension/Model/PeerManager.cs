using System;
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
            List<Peer> output = new List<Peer>();
            foreach (Peer p in allPeers)
                if (p.circles.Contains(id))
                    output.Add(p);
            return output.ToArray();
        }
        public delegate void PeerUpdateEvent(Peer p);
        public event PeerUpdateEvent peerRenamed;
        public event PeerUpdateEvent peerAdded;
        Dictionary<ulong, Peer> peers = new Dictionary<ulong, Peer>();
        public void parseHello(Commands.HelloCommand h, System.Net.IPEndPoint sender)
        {
            bool renamed = false;
            bool added = false;
            lock (peers)
            {
                if (peers.ContainsKey(h.id))
                {
                    peers[h.id].actualEndpoint = sender;
                    if(h.externalIP != null)
                        peers[h.id].publicAddress = System.Net.IPAddress.Parse(h.externalIP);
                    if (peers[h.id].username != h.username)
                    {
                        peers[h.id].username = h.username;
                        renamed = true;
                    }
                    if (peers[h.id].share != h.myShare)
                    {
                        peers[h.id].share = h.myShare;
                        renamed = true;
                    }
                    string s1 = "";
                    string s2 = "";
                    foreach (int i in peers[h.id].circles)
                        s1 += i.ToString() + ", ";
                    foreach (int i in h.myCircles)
                        s2 += i.ToString() + ", ";
                    if (s1 != s2)
                    {
                        peers[h.id].circles = h.myCircles;
                        added = true;
                    }
                }
                else
                {
                    peers[h.id] = new Peer();
                    peers[h.id].id = h.id;
                    peers[h.id].actualEndpoint = sender;
                    if(h.externalIP != null)
                        peers[h.id].publicAddress = System.Net.IPAddress.Parse(h.externalIP);
                    peers[h.id].username = h.username;
                    peers[h.id].circles = h.myCircles;
                    added = true;
                }
            }
            if(renamed)                peerRenamed?.Invoke(peers[h.id]);
            if(added)                  peerAdded?.Invoke(peers[h.id]);
        }
    }
}
