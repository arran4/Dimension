using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model.Commands
{
    public class HelloCommand : Command
    {
        public string username;
        public string machineName;
        public int peerCount;
        public int commandPort;
        public int dataPort;
        public System.Net.IPAddress externalIP;
        public System.Net.IPAddress internalIP;
    }
}
