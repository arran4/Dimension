using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    class UdtIncomingConnection : IncomingConnection
    {
        public override event CommandReceived commandReceived;
        public UdtIncomingConnection(Udt.Socket s)
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
