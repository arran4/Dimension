﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    class LoopbackOutgoingConnection : OutgoingConnection
    {
        public override event CommandReceived commandReceived;
        LoopbackIncomingConnection c;
        public LoopbackOutgoingConnection()
        {
            c = new LoopbackIncomingConnection(this);
            Program.theCore.addIncomingConnection(c);
        }
        public override void send(Commands.Command c)
        {
            this.c.received(c);
        }
        public void received(Commands.Command c)
        {
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
