using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    class PeerManager
    {
        Dictionary<ulong, Peer> peers = new Dictionary<ulong, Peer>();
        public void parseHello(Commands.HelloCommand h, System.Net.IPEndPoint sender)
        {
            if (peers.ContainsKey(h.id))
            {
                peers[h.id].actualEndpoint = sender;
                peers[h.id].publicAddress = System.Net.IPAddress.Parse(h.externalIP);
                if (peers[h.id].username != h.username)
                {
                    peers[h.id].username = h.username;
                    //TODO: Trigger event for username change
                }
            }
            else
            {
                peers[h.id] = new Peer();
                peers[h.id].id = h.id;
                peers[h.id].actualEndpoint = sender;
                peers[h.id].publicAddress = System.Net.IPAddress.Parse(h.externalIP);
                peers[h.id].username = h.username;
                //TODO: Trigger event for new peer found
            }
        }
    }
}
