/*
 * Original C# Source File: DimensionLib/Model/LoopbackIncomingConnection.cs
 *
﻿using System;
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
                App.globalUpCounter.addBytes(((Commands.DataCommand)c).data.Length);
                upCounter.addBytes(((Commands.DataCommand)c).data.Length);
                App.speedLimiter.limitUpload((ulong)((Commands.DataCommand)c).data.Length, rateLimiterDisabled);
            }
            o.received(c);
        }
        public void received(Commands.Command c)
        {
            if (c is Commands.DataCommand)
            {
                App.globalDownCounter.addBytes(((Commands.DataCommand)c).data.Length);
                downCounter.addBytes(((Commands.DataCommand)c).data.Length);
                App.speedLimiter.limitDownload((ulong)((Commands.DataCommand)c).data.Length, rateLimiterDisabled);
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

*/
