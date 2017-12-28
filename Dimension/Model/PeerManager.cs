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
                    peers[h.id].publicAddress = System.Net.IPAddress.Parse(h.externalIP);
                    if (peers[h.id].username != h.username)
                    {
                        peers[h.id].username = h.username;
                        renamed = true;
                    }
                }
                else
                {
                    peers[h.id] = new Peer();
                    peers[h.id].id = h.id;
                    peers[h.id].actualEndpoint = sender;
                    peers[h.id].publicAddress = System.Net.IPAddress.Parse(h.externalIP);
                    peers[h.id].username = h.username;
                    added = true;
                }
            }
            if(renamed)                peerRenamed?.Invoke(peers[h.id]);
            if(added)                  peerAdded?.Invoke(peers[h.id]);
        }
    }
}
