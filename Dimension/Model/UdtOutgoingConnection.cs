using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    class UdtOutgoingConnection : OutgoingConnection
    {
        public override event CommandReceived commandReceived;
        public UdtOutgoingConnection(System.Net.IPAddress addr, int port)
        {

        }


        public override void send(Commands.Command c)
        {

        }
        public override bool connected
        {
            get
            {
                return false;
            }
        }
    }
}
