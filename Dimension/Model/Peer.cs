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
        public OutgoingConnection udtConnection;
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
        public int localUDTPort;
        bool isLocal
        {
            get
            {
                //TODO: Add more conditions
                if (Program.bootstrap.publicControlEndPoint == null)
                    return true;
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

            if (c is Commands.FileChunk)
            {
                var chunk = (Commands.FileChunk)c;
                string s = Program.settings.getString("Default Download Folder", "C:\\Downloads");
                if (!System.IO.Directory.Exists(s))
                    System.IO.Directory.CreateDirectory(s);
                System.IO.FileStream f = new System.IO.FileStream(s + "\\" + chunk.path.Substring(chunk.path.LastIndexOf("/") + 1), System.IO.FileMode.OpenOrCreate);
                f.Seek(chunk.start, System.IO.SeekOrigin.Begin);
                f.Write(chunk.data, 0, chunk.data.Length);
                f.Close();

            }
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
            bool createUdt = true;
            if (udtConnection != null)
                if (udtConnection.connected)
                    createUdt = false;
            if (id == Program.theCore.id)
            {
                if (createControl)
                    controlConnection = new LoopbackOutgoingConnection();
                if (createData)
                    dataConnection = new LoopbackOutgoingConnection();
                createUdt = false;
            }
            else
            {
                if (isLocal)
                {
                    if (createUdt)
                        udtConnection = new UdtOutgoingConnection(actualEndpoint.Address, localUDTPort);
                    if (createControl)
                        controlConnection = new ReliableOutgoingConnection(actualEndpoint.Address, localDataPort);
                    if (createData)
                        dataConnection = new ReliableOutgoingConnection(actualEndpoint.Address, localDataPort);
                }
                else
                {

                    createUdt = false;
                    if (createControl)
                        controlConnection = new ReliableOutgoingConnection(actualEndpoint.Address, externalDataPort);
                    if (createData)
                        dataConnection = new ReliableOutgoingConnection(actualEndpoint.Address, externalDataPort);
                }

            }
            if (createData)
                dataConnection.commandReceived += commandReceived;
            if (createControl)
                controlConnection.commandReceived += commandReceived;
            if (createUdt)
                udtConnection.commandReceived += commandReceived;
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
