using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    class PeerManager
    {
        public Peer[] allPeers
        {
            get
            {
                lock (peers)
                    return ((new List<Peer>(peers.Values)).ToArray());
            }
        }
        public delegate void PeerUpdateEvent(Peer p);
        public event PeerUpdateEvent peerRenamed;
        public event PeerUpdateEvent peerAdded;
        Dictionary<ulong, Peer> peers = new Dictionary<ulong, Peer>();
        public void parseHello(Commands.HelloCommand h, System.Net.IPEndPoint sender)
        {
            lock (peers)
            {
                if (peers.ContainsKey(h.id))
                {
                    peers[h.id].actualEndpoint = sender;
                    peers[h.id].publicAddress = System.Net.IPAddress.Parse(h.externalIP);
                    if (peers[h.id].username != h.username)
                    {
                        peers[h.id].username = h.username;
                        if (peerRenamed != null)
                            peerRenamed(peers[h.id]);
                    }
                }
                else
                {
                    peers[h.id] = new Peer();
                    peers[h.id].id = h.id;
                    peers[h.id].actualEndpoint = sender;
                    peers[h.id].publicAddress = System.Net.IPAddress.Parse(h.externalIP);
                    peers[h.id].username = h.username;
                    if (peerAdded != null)
                        peerAdded(peers[h.id]);
                }
            }
        }
    }
}
