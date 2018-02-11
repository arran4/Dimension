using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model.Commands
{
    public abstract class PunchCommand : Command
    {
        public ulong myId;
    }
}
