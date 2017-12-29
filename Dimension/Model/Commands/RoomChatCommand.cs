using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model.Commands
{
    public class RoomChatCommand : Command
    {
        public string content;
        public long sequenceId;
    }
}
