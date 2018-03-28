using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model.Commands
{
    class ReverseConnectionType : Command
    {
        public bool makeControl = false;
        public bool makeData = false;
        public ulong id;
    }
}
