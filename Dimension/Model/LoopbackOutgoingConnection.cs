using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    class LoopbackOutgoingConnection : OutgoingConnection
    {
        public ByteCounter upCounter = new ByteCounter();
        public ByteCounter downCounter = new ByteCounter();
        public override event CommandReceived commandReceived;
        LoopbackIncomingConnection c;
        public LoopbackOutgoingConnection()
        {
            c = new LoopbackIncomingConnection(this);
            Program.theCore.addIncomingConnection(c);
        }
        public override void send(Commands.Command c)
        {
            if (c is Commands.DataCommand)
            {
                Program.globalUpCounter.addBytes(((Commands.DataCommand)c).data.Length);
                upCounter.addBytes(((Commands.DataCommand)c).data.Length);
            }
            this.c.received(c);
        }
        public void received(Commands.Command c)
        {
            if (c is Commands.DataCommand)
            {
                Program.globalDownCounter.addBytes(((Commands.DataCommand)c).data.Length);
                downCounter.addBytes(((Commands.DataCommand)c).data.Length);
            }
            commandReceived?.Invoke(c);
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
