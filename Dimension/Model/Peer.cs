using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    public class Peer
    {
        public DateTime lastTimeHelloSent = DateTime.MinValue;
        public Dictionary<ulong, int> peerCount = new Dictionary<ulong, int>();
        public OutgoingConnection dataConnection;
        public OutgoingConnection controlConnection;
        public OutgoingConnection udtConnection;
        public System.Net.IPAddress publicAddress;
        public System.Net.IPAddress[] internalAddress;
        public int buildNumber;
        System.Net.IPAddress _actualAddr;
        public System.Net.IPEndPoint actualEndpoint
        {
            get
            {
                return new System.Net.IPEndPoint(_actualAddr, isLocal ? localControlPort : externalControlPort);
            }
            set
            {
                _actualAddr = value.Address;
            }
        }
        public string username;
        public ulong id;
        public ulong[] circles;
        public ulong share;
        public int externalDataPort;
        public int externalControlPort;
        public int localDataPort;
        public int localControlPort;
        public int localUDTPort;
        public bool useUDT;
        public bool isLocal
        {
            get
            {
                //TODO: Add more conditions
                if (id == Program.theCore.id)
                    return true;
                if (Program.bootstrap.publicControlEndPoint == null)
                    return true;
                if (publicAddress.ToString() == Program.bootstrap.publicControlEndPoint.Address.ToString())
                    return true;
                return false;
            }
        }
        public Dictionary<string, Transfer> transfers = new Dictionary<string, Transfer>();
        public bool quit = false;
        public delegate void CommandReceived(Commands.Command c);
        public event CommandReceived commandReceivedEvent;
        public void commandReceived(Commands.Command c)
        {
            commandReceivedEvent?.Invoke(c);

            if (c is Commands.PrivateChatCommand)
            {
                Commands.PrivateChatCommand chat = (Commands.PrivateChatCommand)c;

            }

            if (c is Commands.CancelCommand)
            {
                OutgoingConnection c3 = null;
                if (udtConnection != null)
                    if (udtConnection.connected)
                        c3 = udtConnection;
                if (dataConnection != null)
                    if (dataConnection.connected)
                        c3 = dataConnection;
                string s = (((Commands.CancelCommand)c).path);
                Transfer.transfers.Remove(transfers[s]);
                transfers.Remove(s);
                c3.send(c);
            }
            if (c is Commands.FileChunk)
            {
                var chunk = (Commands.FileChunk)c;
                
                string s = Program.settings.getString("Default Download Folder", "C:\\Downloads");
                if (!System.IO.Directory.Exists(s))
                    System.IO.Directory.CreateDirectory(s);

                System.Diagnostics.Stopwatch loopbackWatch = new System.Diagnostics.Stopwatch();
                loopbackWatch.Start();
                int attempts = 0;
                tryAgain:
                try
                {
                    System.IO.FileStream f = new System.IO.FileStream(s + "\\" + chunk.path.Substring(chunk.path.LastIndexOf("/") + 1), System.IO.FileMode.OpenOrCreate);
                    f.Seek(chunk.start, System.IO.SeekOrigin.Begin);
                    f.Write(chunk.data, 0, chunk.data.Length);
                    f.Close();
                }
                catch(Exception e)
                {
                    //if (attempts > 10)
                        //throw e;
                    System.Threading.Thread.Sleep(50);
                    attempts++;
                    goto tryAgain;
                }
                loopbackWatch.Stop();
                if(transfers.ContainsKey(chunk.path)){
                    Transfer t = transfers[chunk.path];
                    if (t != null)
                    {
                        t.path = chunk.path;
                        t.completed += (ulong)chunk.data.Length;
                        if (t.completed >= t.size)
                        {
                            lock (Transfer.transfers)
                            {
                                transfers.Remove(chunk.path);
                                Transfer.transfers.Remove(t);
                                t = null;
                            }
                        }
                        else
                        {
                            OutgoingConnection c3 = null;
                            if (udtConnection != null)
                                if (udtConnection.connected)
                                    c3 = udtConnection;
                            if (dataConnection != null)
                                if (dataConnection.connected)
                                    if (dataConnection.rate > 0)
                                        c3 = dataConnection;
                            if (id == Program.theCore.id)
                            {
                                t.rate = ((LoopbackOutgoingConnection)dataConnection).downCounter.frontBuffer;
                                t.username = username;
                            }
                            if (c3 != null)
                            {
                                if (c3.rate > 0)
                                    t.rate = c3.rate;
                            }
                        }
                    }
                }
                }
        }

        public void sendCommand(Commands.Command c)
        {
            byte[] b = Program.serializer.serialize(c);
            if (isLocal)
            {
                foreach(System.Net.IPAddress a in internalAddress)
                    Program.udp.Send(b, b.Length, new System.Net.IPEndPoint(a, localControlPort));
            }
            else
                Program.udp.Send(b, b.Length, actualEndpoint);
            Program.globalUpCounter.addBytes(b.Length);
        }
        public void reverseConnect()
        {
            int port = externalDataPort;
            if (isLocal)
                port = localDataPort;

            var t = new System.Net.Sockets.TcpClient();
            try
            {
                t.Connect(new System.Net.IPEndPoint(actualEndpoint.Address, port));
            }
            catch
            {
                SystemLog.addEntry("Error reverse connecting to " + actualEndpoint.Address + ":" + port.ToString());
                return;
            }
                ReliableIncomingConnection c = new ReliableIncomingConnection(t);
            c.send(new Commands.ReverseConnectionType() { makeControl = true, id = Program.theCore.id });
            Program.theCore.addIncomingConnection(c);

            t = new System.Net.Sockets.TcpClient();
            try
            {
                t.Connect(new System.Net.IPEndPoint(actualEndpoint.Address, port));
            }
            catch
            {
                SystemLog.addEntry("Error reverse connecting to " + actualEndpoint.Address + ":" + port.ToString());
                return;
            }
            c = new ReliableIncomingConnection(t);
            c.send(new Commands.ReverseConnectionType() { makeData = true, id = Program.theCore.id });
            Program.theCore.addIncomingConnection(c);
        }
        public delegate void MessageResponseDelegate(string s);
        public void createConnection(MessageResponseDelegate response = null)
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
            if (!useUDT || !Program.settings.getBool("Use UDT", true))
                createUdt = false;
            if (id == Program.theCore.id)
            {
                response?.Invoke("Loopback peer found, creating loopback connection...");
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
                    response?.Invoke("Local peer found.");
                    if (createUdt)
                    {
                        response?.Invoke("Creating UDT connection to "+ actualEndpoint.Address.ToString()+":"+localUDTPort.ToString());
                        udtConnection = new UdtOutgoingConnection(actualEndpoint.Address, localUDTPort);
                    }
                    if (createControl)
                    {
                        response?.Invoke("Creating TCP connection to " + actualEndpoint.Address.ToString() + ":" + localDataPort.ToString());
                        controlConnection = new ReliableOutgoingConnection(actualEndpoint.Address, localDataPort);
                    }
                    if (createData)
                    {
                        response?.Invoke("Creating TCP connection to " + actualEndpoint.Address.ToString() + ":" + localDataPort.ToString());
                        dataConnection = new ReliableOutgoingConnection(actualEndpoint.Address, localDataPort);

                    }
                }
                else
                {

                    createUdt = false;
                    try
                    {
                        if (createControl)
                        {
                            response?.Invoke("Creating TCP connection to " + actualEndpoint.Address.ToString() + ":" + externalDataPort.ToString());
                            controlConnection = new ReliableOutgoingConnection(actualEndpoint.Address, externalDataPort);
                        }
                        if (createData)
                        {
                            response?.Invoke("Creating TCP connection to " + actualEndpoint.Address.ToString() + ":" + externalDataPort.ToString());
                            dataConnection = new ReliableOutgoingConnection(actualEndpoint.Address, externalDataPort);
                        }
                    }
                    catch (System.Net.Sockets.SocketException s)
                    {
                        response?.Invoke("Error: " + s.Message + ".");
                        response?.Invoke("Attempting to initiate reverse connection.");

                        byte[] b = Program.serializer.serialize(new Commands.ConnectToMe());
                        Program.udp.Send(b, b.Length, actualEndpoint);
                        Program.globalUpCounter.addBytes(b.Length);
                        return;
                    }
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

            foreach (string s in r.content.Split('\n'))
            {
                if (s.StartsWith("/me"))
                {
                    Program.theCore.chatReceived(DateTime.Now.ToShortTimeString() + " *** " + username + " " + s.Trim('\r').Substring(4), r.roomId);

                }
                else
                {
                    Program.theCore.chatReceived(DateTime.Now.ToShortTimeString() + " " + username + ": " + s.Trim('\r'), r.roomId);
                }
            }

        }
    }
}
