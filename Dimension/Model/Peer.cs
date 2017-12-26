using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    class Peer
    {
        public System.Net.IPEndPoint publicEndpoint;
        public System.Net.IPEndPoint localEndpoint;
        public string username;
        public ulong id;
    }
}
