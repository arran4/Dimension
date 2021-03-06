﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model.Commands
{
    public class HelloCommand : Command
    {
        public bool? debugBuild;
        public bool? afk;
        public ulong id;
        public ulong myShare;
        public string username;
        public string description;
        public Dictionary<ulong, int> peerCount;
        public int externalControlPort;
        public int externalDataPort;
        public int internalControlPort;
        public int internalDataPort;
        public int internalUdtPort;
        public string externalIP;
        public ulong[] myCircles;
        public bool useUDT;
        public string[] internalIPs;
        public int buildNumber;
        public bool behindDoubleNAT;
        public bool requestingHelloBack = false;
        public string[] extensions = new string[] { };
    }
}
