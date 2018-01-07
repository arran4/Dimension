using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    class LoopbackIncomingConnection : IncomingConnection
    {
        public ByteCounter upCounter = new ByteCounter();
        public ByteCounter downCounter = new ByteCounter();
        public override event CommandReceived commandReceived;
        LoopbackOutgoingConnection o;
        public LoopbackIncomingConnection(LoopbackOutgoingConnection o)
        {
            this.o = o;
        }
        public override void send(Commands.Command c)
        {
            if (c is Commands.DataCommand)
            {
                Program.globalUpCounter.addBytes(((Commands.DataCommand)c).data.Length);
                upCounter.addBytes(((Commands.DataCommand)c).data.Length);
                Program.speedLimiter.limitUpload((ulong)((Commands.DataCommand)c).data.Length);
            }
            o.received(c);
        }
        public void received(Commands.Command c)
        {
            if (c is Commands.DataCommand)
            {
                Program.globalDownCounter.addBytes(((Commands.DataCommand)c).data.Length);
                downCounter.addBytes(((Commands.DataCommand)c).data.Length);
                Program.speedLimiter.limitDownload((ulong)((Commands.DataCommand)c).data.Length);
            }
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
