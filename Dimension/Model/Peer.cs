using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    public class Peer
    {
        public DateTime timeQuit = DateTime.MinValue;
        public DateTime lastContact = DateTime.Now;
        public DateTime lastTimeHelloSent = DateTime.MinValue;
        public Dictionary<ulong, int> peerCount = new Dictionary<ulong, int>();
        public OutgoingConnection dataConnection;
        public OutgoingConnection controlConnection;
        public OutgoingConnection udtConnection;
        public System.Net.IPAddress publicAddress;
        public System.Net.IPAddress[] internalAddress;
        public int buildNumber;
        public string description;
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
        public DateTime lastGossipTime = DateTime.MinValue;
        public Dictionary<ulong, int> lastGossipPeerCount = new Dictionary<ulong, int>();
        public bool? afk;
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
        public bool behindDoubleNAT;

        public bool maybeDead
        {
            get
            {
                if (DateTime.Now.Subtract(lastContact).TotalMilliseconds > 5000)
                    return true;
                else
                    return false;
            }
        }
        public bool probablyDead
        {
            get
            {
                if (DateTime.Now.Subtract(lastContact).TotalMilliseconds > 20000)
                    return true;
                else
                    return false;
            }
        }
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

        public static string downloadFilePath(string s)
        {
            string s2 = Program.settings.getString("Default Download Folder", "C:\\Downloads");
            string x = s;
            if (x.Contains("/"))
                x = x.Substring(x.LastIndexOf("/") + 1);
            return System.IO.Path.Combine(s2, x);
        }
        public void commandReceived(Commands.Command c)
        {
            commandReceivedEvent?.Invoke(c);

            if (c is Commands.PrivateChatCommand)
            {
                Commands.PrivateChatCommand chat = (Commands.PrivateChatCommand)c;
                Program.mainForm.privateChatReceived((Commands.PrivateChatCommand)c, this);

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
                lock (transfers)
                {
                    if (transfers.ContainsKey(s))
                    {
                        Transfer.transfers.Remove(transfers[s]);

                        transfers.Remove(s);
                    }
                }
                c3.send(c);
            }
            if (c is Commands.FileChunk)
            {
                var chunk = (Commands.FileChunk)c;
                
                string s = Program.settings.getString("Default Download Folder", "C:\\Downloads");
                if (!System.IO.Directory.Exists(s))
                    System.IO.Directory.CreateDirectory(s);
                System.IO.DirectoryInfo d = new System.IO.DirectoryInfo(s);

                string subfolderName = chunk.originalPath.TrimEnd('/');
                if (chunk.originalPath.Contains("/"))
                    subfolderName = subfolderName.Substring(subfolderName.LastIndexOf("/")+1);

                string filename;
                if (chunk.originalPath == chunk.path)
                {
                    filename = downloadFilePath(chunk.path);
                }
                else
                {
                    string subfolderRealName = System.IO.Path.Combine(d.FullName, subfolderName);
                    if (!System.IO.Directory.Exists(subfolderRealName))
                        System.IO.Directory.CreateDirectory(subfolderRealName);
                    System.IO.DirectoryInfo currentFolder = new System.IO.DirectoryInfo(subfolderRealName);
                    string[] remainingPath = chunk.path.TrimEnd('/').Substring(chunk.originalPath.TrimEnd('/').Length + 1).Split('/');

                    for (int i = 0; i < remainingPath.Length - 1; i++)
                    {
                        string blah = System.IO.Path.Combine(currentFolder.FullName, remainingPath[i]);
                        if (!System.IO.Directory.Exists(blah))
                            currentFolder = System.IO.Directory.CreateDirectory(blah);
                    }

                    filename = currentFolder.FullName + "\\" + chunk.path.Substring(chunk.path.LastIndexOf("/") + 1);

                }
                System.Diagnostics.Stopwatch loopbackWatch = new System.Diagnostics.Stopwatch();
                loopbackWatch.Start();
                int attempts = 0;
                tryAgain:
                try
                {
                    System.IO.FileStream f = new System.IO.FileStream(filename, System.IO.FileMode.OpenOrCreate);
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
                lock (transfers)
                {
                    if (transfers.ContainsKey(chunk.originalPath))
                    {
                        Transfer t = transfers[chunk.originalPath];
                        if (t != null)
                        {
                            t.path = chunk.path;
                            t.completed += (ulong)chunk.data.Length;
                            if (t.completed >= t.size)
                            {
                                lock (Transfer.transfers)
                                {
                                    transfers.Remove(chunk.originalPath);
                                    Transfer.transfers.Remove(t);
                                    t = null;
                                }
                            }
                        }
                    }
                }
                }
        }
        public void updateTransfers()
        {
            lock (transfers)
                foreach (string s in transfers.Keys)
                    updateTransferRate(transfers[s]);
        }

        void updateTransferRate(Transfer t)
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
                t.userId = id;
            }
            if (c3 != null)
            {
                if (c3.rate > 0)
                    t.rate = c3.rate;
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
                    if (internalAddress.Length > 1)
                    {
                        response?.Invoke("Multiple local addresses found. Trying each...");
                    }

                    for (int i = 0; i < internalAddress.Length; i++)
                    {
                        actualEndpoint = new System.Net.IPEndPoint(internalAddress[i], localControlPort);
                        try
                        {
                            if (createUdt && udtConnection == null)
                            {
                                response?.Invoke("Creating UDT connection to " + actualEndpoint.Address.ToString() + ":" + localUDTPort.ToString());
                                udtConnection = new UdtOutgoingConnection(actualEndpoint.Address, localUDTPort);
                            }
                            if (createControl && controlConnection == null)
                            {
                                response?.Invoke("Creating TCP connection to " + actualEndpoint.Address.ToString() + ":" + localDataPort.ToString());
                                controlConnection = new ReliableOutgoingConnection(actualEndpoint.Address, localDataPort);
                            }
                            if (createData && dataConnection == null)
                            {
                                response?.Invoke("Creating TCP connection to " + actualEndpoint.Address.ToString() + ":" + localDataPort.ToString());
                                dataConnection = new ReliableOutgoingConnection(actualEndpoint.Address, localDataPort);

                            }
                        }
                        catch (Exception e)
                        {
                            SystemLog.addEntry("Failed to connect to " + actualEndpoint.Address.ToString());
                        }
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
                if(dataConnection != null)
                    dataConnection.commandReceived += commandReceived;
            if (createControl && controlConnection != null)
                controlConnection.commandReceived += commandReceived;
            if (createUdt && udtConnection != null)
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
                string s2 = s;
                if (s2.Contains("hunter2"))
                    s2 = s2.Replace("hunter2", "*******");
                if (r.content.ToLower().Contains(Program.settings.getString("Username", Environment.MachineName)))
                    Program.mainForm.flash();
                if (s2.StartsWith("/me"))
                {
                    Program.theCore.chatReceived(DateTime.Now.ToShortTimeString() + " *** " + username + " " + s2.Trim('\r').Substring(4), r.roomId, this);

                }
                else
                {
                    Program.theCore.chatReceived(DateTime.Now.ToShortTimeString() + " " + username + ": " + s2.Trim('\r'), r.roomId, this);
                }
            }

        }
    }
}
