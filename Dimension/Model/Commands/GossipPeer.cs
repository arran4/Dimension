using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model.Commands
{
    public class GossipPeer
    {
        public string[] internalAddresses;
        public string internalAddress;
        public string publicAddress;
        public int internalControlPort;
        public int publicControlPort;
    }
}
