﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    public abstract class OutgoingConnection
    {
        public abstract bool connected
        {
            get;
        }
    }
}
