﻿using System;
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
        public System.Net.IPAddress publicAddress;
        public System.Net.IPAddress[] internalAddress;
        public int buildNumber;
        public string description;
        System.Net.IPAddress _actualAddr;
        List<System.Net.IPEndPoint> endpointHistory = new List<System.Net.IPEndPoint>();

        public void downloadElement(string s, Model.Commands.FSListing tag)
        {

            bool useUDT = false;

            Model.Transfer t;
            lock (transfers)
            {
                if (!transfers.ContainsKey(s))
                {
                    t = new Model.Transfer();
                    t.thePeer = this;
                    transfers[s] = t;
                    t.originalPath = s;
                    t.path = s;
                    t.username = username;
                    t.userId = id;
                    t.filename = tag.name;
                    t.download = true;
                    t.size = tag.size;
                    t.completed = 0;
                    if (dataConnection is Model.LoopbackOutgoingConnection)
                        t.protocol = "Loopback";
                    else
                        if (dataConnection is Model.ReliableOutgoingConnection)
                        t.protocol = "TCP";
                    else
                        t.protocol = "UDT";

                    transfers[t.path] = t;
                    lock (Model.Transfer.transfers)
                        Model.Transfer.transfers.Add(t);
                }
                else
                    t = transfers[s];
            }
            System.Threading.Thread t2 = new System.Threading.Thread(delegate ()
            {
                long startingByte = 0;
                string downloadPath = Model.Peer.downloadFilePath(s);
                if (System.IO.File.Exists(downloadPath + ".incomplete"))
                {
                    startingByte = new System.IO.FileInfo(downloadPath + ".incomplete").Length;
                    t.completed = (ulong)startingByte;
                    t.startingByte = t.completed;
                }
                if (startingByte >= (long)tag.size) //we already have the file
                {
                    transfers.Remove(t.path);
                    lock (Model.Transfer.transfers)
                        Model.Transfer.transfers.Remove(t);
                    return;
                }
                Model.Commands.Command c;
                if (tag.isFolder)
                {
                    c = new Model.Commands.RequestFolderContents { path = s };
                }
                else
                {
                    c = new Model.Commands.RequestChunks() { allChunks = true, path = s, startingByte = startingByte };
                }
                t.con = dataConnection;
                dataConnection.send(c);

            });
            t2.IsBackground = true;
            t2.Name = "Download request thread";
            t2.Start();
        }
        public bool endpointIsInHistory(System.Net.IPEndPoint ep)
        {
            lock (endpointHistory)
            {
                foreach (System.Net.IPEndPoint e in endpointHistory)
                    if (e.Address.ToString() == ep.Address.ToString() && e.Port == ep.Port)
                        return true;
                return false;
            }
        }
        public void addEndpointToHistory(System.Net.IPEndPoint ep)
        {
            lock (endpointHistory)
            {
                foreach (System.Net.IPEndPoint e in endpointHistory)
                    if (e.Address.ToString() == ep.Address.ToString() && e.Port == ep.Port)
                        return;
                endpointHistory.Add(ep);
            }
        }
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
                if (DateTime.Now.Subtract(lastContact).TotalMilliseconds > 15000)
                {
                    highestId = -1;
                    return true;
                }
                else
                    return false;
            }
        }
        public bool probablyDead
        {
            get
            {
                if (DateTime.Now.Subtract(lastContact).TotalMilliseconds > 30000)
                {
                    highestId = -1;
                    return true;
                }
                else
                    return false;
            }
        }
        public bool assumingDead
        {
            get
            {
                if (DateTime.Now.Subtract(lastContact).TotalMilliseconds > 60000)
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
                if (id == App.theCore.id)
                    return true;
                if (App.bootstrap.publicDataEndPoint != null)
                    if (publicAddress.ToString() == App.bootstrap.publicDataEndPoint.Address.ToString())
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
            string s2 = App.settings.getString("Default Download Folder", "C:\\Downloads");
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
                App.doPrivateChatReceived((Commands.PrivateChatCommand)c, this);

            }

            if (c is Commands.CancelCommand)
            {
                OutgoingConnection c3 = null;
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
             
                string s = App.settings.getString("Default Download Folder", "C:\\Downloads");
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
                    System.IO.FileStream f = new System.IO.FileStream(filename + ".incomplete", System.IO.FileMode.OpenOrCreate);
                    f.Seek(chunk.start, System.IO.SeekOrigin.Begin);
                    f.Write(chunk.data, 0, chunk.data.Length);
                    if (f.Length >= chunk.totalSize && chunk.totalSize > 0)
                    {
                        f.Close();
                        System.IO.File.Move(filename + ".incomplete", filename);
                    }
                    else
                    {
                        f.Close();
                    }
                }
                catch (Exception e)
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
            if (dataConnection != null)
                if (dataConnection.connected)
                    if (dataConnection.rate > 0)
                        c3 = dataConnection;
            if (id == App.theCore.id)
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
            byte[] b = App.serializer.serialize(c);
            if (isLocal)
            {
                foreach(System.Net.IPAddress a in internalAddress)
                    App.udpSend(b, b.Length, new System.Net.IPEndPoint(a, localControlPort));
            }
            else
                App.udpSend(b, b.Length, actualEndpoint);
        }
        public void reverseConnect()
        {
            System.Net.IPAddress addr = publicAddress;

            var t = attemptConnection();
            if (t != null)
            {
                ReliableIncomingConnection c = new ReliableIncomingConnection(t);
                c.send(new Commands.ReverseConnectionType() { makeControl = true, id = App.theCore.id });
                App.theCore.addIncomingConnection(c);
            }
            t = attemptConnection();
            if (t != null)
            {

                ReliableIncomingConnection c = new ReliableIncomingConnection(t);
                c.send(new Commands.ReverseConnectionType() { makeData = true, id = App.theCore.id });
                App.theCore.addIncomingConnection(c);
            }
        }

        System.Net.Sockets.TcpClient attemptConnection()
        {

            var t = new System.Net.Sockets.TcpClient();

            try
            {
                if (isLocal)
                    t.Connect(new System.Net.IPEndPoint(_actualAddr, localDataPort));
                else
                    t.Connect(new System.Net.IPEndPoint(_actualAddr, externalDataPort));
                return t;
            }
            catch
            {

            }
            try
            {
                if (isLocal)
                {
                    for (int i = 0; i < internalAddress.Length; i++)
                    {
                        try
                        {
                            t.Connect(new System.Net.IPEndPoint(internalAddress[i], localDataPort));
                            return t;
                        }
                        catch
                        {

                        }
                    }
                }
                else
                {
                    t.Connect(new System.Net.IPEndPoint(publicAddress, externalDataPort));
                    return t;
                }
            }
            catch
            {
                
            }

            SystemLog.addEntry("Error reverse connecting to " + _actualAddr);

            return null;

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
            if (createControl == false && createData == false)
                return;
            if (id == App.theCore.id)
            {
                response?.Invoke("Loopback peer found, creating loopback connection...");
                if (createControl)
                    controlConnection = new LoopbackOutgoingConnection();
                if (createData)
                    dataConnection = new LoopbackOutgoingConnection();
            }
            else
            {
                if (isLocal && internalAddress != null)
                {
                    response?.Invoke("Local peer found.");
                    if (internalAddress.Length > 1)
                    {
                        response?.Invoke("Multiple local addresses found. Trying each...");
                    }

                    for (int i = 0; i < internalAddress.Length; i++)
                    {
                        actualEndpoint = new System.Net.IPEndPoint(internalAddress[i], localControlPort);

                        bool reverseConnect = false;
                        if (App.settings.getBool("Default to Reverse Connection", false))
                            reverseConnect = true;
                        bool rendezvousConnect = false;
                        if (App.settings.getBool("Always Rendezvous", false) && useUDT)
                        {
                            reverseConnect = false;
                            rendezvousConnect = true;
                        }
                        if (!reverseConnect && !rendezvousConnect)
                        {
                            try
                            {
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
                                reverseConnect = true;
                            }
                        }
                        if (reverseConnect)
                        {
                            doReverseConnection(response);
                            return;
                        }
                        if (rendezvousConnect)
                        {
                            doRendezvous(response);
                            return;
                        }
                    }
                }
                else
                {
                    
                    bool reverseConnect = false;
                    if (App.settings.getBool("Default to Reverse Connection", false))
                        reverseConnect = true;
                    bool rendezvousConnect = false;
                    if (App.settings.getBool("Always Rendezvous", false) && useUDT)
                    {
                        reverseConnect = false;
                        rendezvousConnect = true;
                    }

                    if (!reverseConnect && !rendezvousConnect)
                    {
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
                            reverseConnect = true;
                        }
                    }
                    if (reverseConnect)
                    {
                        doReverseConnection(response);
                        return;
                    }
                    if (rendezvousConnect)
                    {
                        doRendezvous(response);
                        return;
                    }
                }

            }
            if (createData)
                if (dataConnection != null && dataConnection != controlConnection)
                    dataConnection.commandReceived += commandReceived;
            if (createControl && controlConnection != null)
                controlConnection.commandReceived += commandReceived;
            System.Threading.Thread.Sleep(500);
        }
        void doRendezvous(MessageResponseDelegate response)
        {

            response?.Invoke("Attempting to initiate rendezvous connection.");

            response?.Invoke("Binding to random socket.");
            System.Net.Sockets.Socket udp = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp);
            udp.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Any, 0));

            response?.Invoke("Successfully bound to UDP endpoint " + udp.LocalEndPoint.ToString());

            response?.Invoke("Sending UDP punch.");
            byte[] b = App.serializer.serialize(new Commands.BeginPunchCommand() { myId = App.theCore.id, port = (ushort)((System.Net.IPEndPoint)udp.LocalEndPoint).Port });
            App.udpSend(b, actualEndpoint);

            response?.Invoke("Waiting for UDP response.");
            rendezvousSemaphore.WaitOne();

            response?.Invoke("Received a UDP response! Remote endpoint is " + rendezvousAddress.ToString());


            response?.Invoke("Binding UDT to UDP socket.");
            Udt.Socket s = new Udt.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream);

            s.ReuseAddress = true;
            s.Bind(udp);

            s.Rendezvous = true;

            response?.Invoke("Performing UDT control rendezvous...");
            try
            {
                s.Connect(rendezvousAddress);
            }
            catch
            {
                return;
            }

            while (s.State == Udt.SocketState.Connecting)
                System.Threading.Thread.Sleep(10);
            controlConnection = new UdtOutgoingConnection(s, udp);
            dataConnection = controlConnection;

            response?.Invoke("Rendezvous connection successful!");
            controlConnection.send(new Model.Commands.GetFileListing("/"));
            
            dataConnection.commandReceived += commandReceived;
            controlConnection.commandReceived += commandReceived;
        }
        void doReverseConnection(MessageResponseDelegate response)
        {

            response?.Invoke("Attempting to initiate reverse connection.");

            byte[] b = App.serializer.serialize(new Commands.ConnectToMe() { myId = App.theCore.id });
            App.udpSend(b, b.Length, actualEndpoint);

            System.Threading.Thread t = new System.Threading.Thread(delegate ()
            {
                System.Threading.Thread.Sleep(15000);

                if (dataConnection == null || controlConnection == null)
                {
                    response?.Invoke("Failed to initiate reverse connection. Attempting rendezvous connection...");
                    doRendezvous(response);
                }
            });
            t.Name = "Reverse connection timeout thread";
            t.IsBackground = true;
            t.Start();
        }
        public void endPunch(System.Net.IPEndPoint sender)
        {
            SystemLog.addEntry("Received BeginPunch from " + sender.ToString());
            System.Net.Sockets.Socket udp = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp);

            udp.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Any, 0));
            SystemLog.addEntry("Bound " + udp.LocalEndPoint.ToString());

            byte[] b = App.serializer.serialize(new Commands.EndPunchCommand() { myId = App.theCore.id, port= (ushort)((System.Net.IPEndPoint)udp.LocalEndPoint).Port });
            if (isLocal)
            {
                foreach (System.Net.IPAddress a in internalAddress)
                {
                    SystemLog.addEntry("Sent EndPunch to " + new System.Net.IPEndPoint(a, localControlPort));
                    App.udpSend(b, new System.Net.IPEndPoint(a, localControlPort));
                }
            }
            else
            {
                SystemLog.addEntry("Sent EndPunch to " + actualEndpoint);
                App.udpSend(b, actualEndpoint);
            }
            System.Threading.Thread t = new System.Threading.Thread(delegate ()
            {
                try
                {
                    Udt.Socket s = new Udt.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream);
                    s.ReuseAddress = true;
                    
                    s.Bind(udp);
                    s.Rendezvous = true;
                    SystemLog.addEntry("Beginning rendezvous...");
                    s.Connect(sender);
                    while (s.State == Udt.SocketState.Connecting)
                        System.Threading.Thread.Sleep(10);
                    App.theCore.addIncomingConnection(new UdtIncomingConnection(s, udp));

                    SystemLog.addEntry("Rendezvous successful!");
                }
                catch (Exception e)
                {
                    SystemLog.addEntry("Error rendezvous'ing to " + sender.ToString() + " - " + e.Message);
                }
            });
            t.Name = "Rendezvous thread";
            t.IsBackground = true;
            t.Start();

        }
        public void releasePunch(System.Net.IPEndPoint sender)
        {
            rendezvousAddress = sender;
            rendezvousSemaphore.Release();
        }
        System.Net.IPEndPoint rendezvousAddress = null;
        System.Threading.Semaphore rendezvousSemaphore = new System.Threading.Semaphore(0, int.MaxValue);
        int highestId = -1;
        public void chatReceived(Commands.RoomChatCommand r)
        {
            if (r.sequenceId <= highestId && Math.Abs(highestId- r.sequenceId) < 1000)
                return;
            highestId = r.sequenceId;
            
            foreach (string s in r.content.Split('\n'))
            {
                string s2 = s;
                if (s2.Contains("hunter2"))
                    s2 = s2.Replace("hunter2", "*******");
                if (r.content.ToLower().Contains(App.settings.getString("Username", Environment.MachineName)))
                    App.doFlash();
                if (s2.StartsWith("/me"))
                {
                    App.theCore.chatReceived(DateTime.Now.ToShortTimeString() + " *** " + username + " " + s2.Trim('\r').Substring(4), r.roomId, this);

                }
                else
                {
                    App.theCore.chatReceived(DateTime.Now.ToShortTimeString() + " " + username + ": " + s2.Trim('\r'), r.roomId, this);
                }
            }

        }
    }
}
