﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model.Commands
{
    public class FSListing
    {
        public string name;
        public string fullPath; //null in direct listings
        public ulong size;
        public bool isFolder;
        public DateTime updated;
    }
}
