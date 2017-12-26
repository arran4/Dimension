using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    class IncomingConnection
    {
        Udt.Socket socket;
        public IncomingConnection(Udt.Socket socket)
        {
            this.socket = socket;
        }
        }
}
