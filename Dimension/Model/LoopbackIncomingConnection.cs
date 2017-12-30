using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    class LoopbackIncomingConnection : IncomingConnection
    {
        public override event CommandReceived commandReceived;
        LoopbackOutgoingConnection o;
        public LoopbackIncomingConnection(LoopbackOutgoingConnection o)
        {
            this.o = o;
        }
        public override void send(Commands.Command c)
        {
            o.received(c);
        }
        public void received(Commands.Command c)
        {
            commandReceived?.Invoke(c, this);
        }
        public override bool connected
        {
            get
            {
                return true;
            }

        }
    }
}
