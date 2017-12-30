using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    public abstract class IncomingConnection
    {
        public delegate void CommandReceived(Commands.Command c, IncomingConnection con);
        public abstract event CommandReceived commandReceived;
        public abstract void send(Commands.Command c);
        public abstract bool connected
        {
            get;
        }
    }
}
