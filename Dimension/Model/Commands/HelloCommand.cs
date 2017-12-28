﻿using System;
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
        public int externalControlPort;
        public int externalDataPort;
        public int internalControlPort;
        public int internalDataPort;
        public string externalIP;
    }
}
