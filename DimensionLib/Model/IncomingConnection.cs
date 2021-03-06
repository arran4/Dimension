﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    public abstract class IncomingConnection : Connection
    {
        public bool rateLimiterDisabled = false;
        public Commands.HelloCommand hello;
        public string lastFolder;
        public delegate void CommandReceived(Commands.Command c, IncomingConnection con);
        public abstract event CommandReceived commandReceived;
        public abstract void send(Commands.Command c);
        public abstract bool connected
        {
            get;
        }
    }
}
