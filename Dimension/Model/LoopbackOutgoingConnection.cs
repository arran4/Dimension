using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    class LoopbackOutgoingConnection : OutgoingConnection
    {
        public override bool connected
        {
            get
            {
                return true;
            }

        }
    }
}
