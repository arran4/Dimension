using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    public class Peer
    {
        public System.Net.IPAddress publicAddress;
        public System.Net.IPEndPoint actualEndpoint;
        public string username;
        public ulong id;
        public ulong[] circles;

        public void sendCommand(Commands.Command c)
        {
            byte[] b = Program.serializer.serialize(c);
            Program.udp.Send(b, b.Length, actualEndpoint);
        }
        List<int> usedIds = new List<int>();
        public void chatReceived(Commands.RoomChatCommand r)
        {
            if (usedIds.Contains(r.sequenceId))
                return;
            usedIds.Add(r.sequenceId);

            Program.theCore.chatReceived(DateTime.Now.ToShortTimeString() + " " + username + ": " + r.content, r.roomId);

        }
        }
}
