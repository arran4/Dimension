using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    public class Peer
    {
        public OutgoingConnection dataConnection;
        public OutgoingConnection controlConnection;
        public System.Net.IPAddress publicAddress;
        public System.Net.IPEndPoint actualEndpoint;
        public string username;
        public ulong id;
        public ulong[] circles;
        public ulong share;
        public int externalDataPort;
        public int externalControlPort;
        public int localDataPort;
        public int localControlPort;
        bool isLocal
        {
            get
            {
                //TODO: Add more conditions
                if (publicAddress.ToString() == Program.bootstrap.publicControlEndPoint.Address.ToString())
                    return true;
                return false;
            }
        }
        public delegate void CommandReceived(Commands.Command c);
        public event CommandReceived commandReceivedEvent;
        public void commandReceived(Commands.Command c)
        {
            commandReceivedEvent?.Invoke(c);
        }
        public void sendCommand(Commands.Command c)
        {
            byte[] b = Program.serializer.serialize(c);
            Program.udp.Send(b, b.Length, actualEndpoint);
        }
        public void createConnection()
        {
            bool createData = true;
            if (dataConnection != null)
                if (dataConnection.connected)
                    createData = false;
            bool createControl = true;
            if (controlConnection != null)
                if (controlConnection.connected)
                    createControl = false;
            if (id == Program.theCore.id)
            {
                if (createControl)
                    controlConnection = new LoopbackOutgoingConnection();
                if (createData)
                    dataConnection = new LoopbackOutgoingConnection();

            }
            else
            {
                if (isLocal)
                {
                    if (createControl)
                        controlConnection = new UdtOutgoingConnection(actualEndpoint.Address, localControlPort);
                    if (createData)
                        controlConnection = new UdtOutgoingConnection(actualEndpoint.Address, localDataPort);
                }
                else
                {
                    if (createControl)
                        controlConnection = new UdtOutgoingConnection(actualEndpoint.Address, externalControlPort);
                    if (createData)
                        controlConnection = new UdtOutgoingConnection(actualEndpoint.Address, externalDataPort);
                }
            }
            dataConnection.commandReceived += commandReceived;
            controlConnection.commandReceived += commandReceived;
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
