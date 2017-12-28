using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    public class Peer
    {
        public System.Net.IPAddress publicAddress;
        public System.Net.IPEndPoint actualEndpoint;
        public string username;
        public ulong id;
        public int[] circles;
    }
}
