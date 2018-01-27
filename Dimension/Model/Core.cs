using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    public class Core : IDisposable
    {
        public List<string> circles = new List<string>();
        public PeerManager peerManager;
        public bool disposed = false;
        public void Dispose()
        {
            disposed = true;

            Model.Commands.Quitting c = new Model.Commands.Quitting();
            c.id = Program.theCore.id;
            byte[] b = Program.serializer.serialize(c);
            foreach (Model.Peer p in Program.theCore.peerManager.allPeers)
            {
                Program.udpSend(b, b.Length, p.actualEndpoint);
                foreach(System.Net.IPAddress ip in p.internalAddress)
                    Program.udpSend(b, b.Length, new System.Net.IPEndPoint(ip,p.localControlPort));
                Program.udpSend(b, b.Length, new System.Net.IPEndPoint(p.publicAddress, p.externalControlPort));
            }
            foreach (System.Net.IPEndPoint e in toHello)
            {
                Program.udpSend(b, b.Length, e);
            }
            }
        public void leaveCircle(string s)
        {
            lock (circles)
                circles.Remove(s);
        }
        public void joinCircle(string s)
        {
            lock (circles)
                circles.Add(s);
        }
        void updateIncomings()
        {
            lock (incomings)
                foreach (IncomingConnection z in incomings)
                    if (z.lastFolder != null)
                        z.send(generateFileListing(z.lastFolder));

        }
        void sendGossip(ulong circleId, System.Net.IPEndPoint target, bool requestingResponse)
        {
            Peer[] allPeers = peerManager.allPeersInCircle(circleId);
            Commands.GossipPeer[] peers = new Commands.GossipPeer[allPeers.Length];
            for (int i = 0; i < peers.Length; i++)
            {
                if (allPeers[i].internalAddress == null)
                    allPeers[i].internalAddress = new System.Net.IPAddress[] { System.Net.IPAddress.Loopback };
                string[] addrs = new string[allPeers[i].internalAddress.Length];
                for (int z = 0; z < addrs.Length; z++)
                    addrs[z] = allPeers[i].internalAddress[z].ToString();
                peers[i] = new Commands.GossipPeer()
                {
                    publicAddress = allPeers[i].publicAddress.ToString(),
                    publicControlPort = allPeers[i].externalControlPort,
                    internalAddress = allPeers[i].internalAddress[0].ToString(),
                    internalControlPort = allPeers[i].localControlPort,
                    internalAddresses = addrs
                };

            }
            byte[] b = Program.serializer.serialize(new Commands.GossipCommand()
            {
                peers = peers,
                requestingGossipBack = requestingResponse,
                circleId = circleId
            });
            Program.udpSend(b, b.Length, target);
        }

        void gossipLoop()
        {
            while (!disposed)
            {
                System.Security.Cryptography.SHA512Managed sha = new System.Security.Cryptography.SHA512Managed();
                lock (circles)
                {
                    ulong lanHash = BitConverter.ToUInt64(sha.ComputeHash(Encoding.UTF8.GetBytes("LAN".ToLower())), 0);
                    foreach (string s in circles)
                    {
                        byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(s.ToLower()));
                        ulong circleId = (BitConverter.ToUInt64(hash, 0));

                        if (circleId == lanHash)
                            continue;
                        List<Peer> potentials = new List<Peer>();

                        Peer[] allPeers = peerManager.allPeersInCircle(circleId, false);

                        Random r = new Random();
                        foreach (Peer p in allPeers)
                            lock (p.peerCount)
                                if (p.peerCount.ContainsKey(circleId))
                                    if (p.peerCount[circleId] != peerManager.allPeersInCircle(circleId, false).Length)
                                        if (p.lastGossipPeerCount == null || DateTime.Now.Subtract(p.lastGossipTime).TotalSeconds > 30)
                                            potentials.Insert(r.Next(0, potentials.Count + 1), p);
                                        else
                                            lock (p.lastGossipPeerCount)
                                                if(p.lastGossipPeerCount.ContainsKey(circleId))
                                                    if (p.lastGossipPeerCount[circleId] != p.peerCount[circleId])
                                                        potentials.Insert(r.Next(0, potentials.Count + 1), p);

                        if (potentials.Count > 0)
                        {
                            sendGossip(circleId, potentials[0].actualEndpoint, true);
                            potentials[0].lastGossipTime = DateTime.Now;
                        }


                        foreach (Peer p in allPeers)
                            lock(p.lastGossipPeerCount)
                                p.lastGossipPeerCount = p.peerCount;

                        System.Threading.Thread.Sleep(1000);
                    }
                }
                System.Threading.Thread.Sleep(1000);
            }

        }
        public bool isMono
        {
            get{
                return Type.GetType("Mono.Runtime") != null;
            }
        }
        public UDTHolder udtHolder;
        public ulong id;
        public Core()
        {
            Model.SystemLog.addEntry("Generating an ID...");
            Random r = new Random();
            ulong randomId = (ulong)r.Next();
            randomId = randomId << 32;
            randomId |= (uint)r.Next();
            id = (ulong)Program.settings.getULong("ID", randomId);
            Program.settings.setULong("ID", id);

            if (!isMono)
            {
                try
                {
                    Model.SystemLog.addEntry("Creating a UDT listener...");
                    udtHolder = new UDTHolder();
                }
                catch
                {
                    Model.SystemLog.addEntry("Microsoft Visual C++ Runtime v10 x86 was not installed. UDT will be disabled.");
                    Program.settings.setBool("Use UDT", false);
                }
            }


            Model.SystemLog.addEntry("Creating a Peer Manager...");
            peerManager = new PeerManager();
            System.Threading.Thread t = new System.Threading.Thread(helloLoop);
            t.IsBackground = true;
            t.Name = "Hello send loop";
            t.Start();

            Model.SystemLog.addEntry("Starting UDP async receive...");
            doReceive(Program.udp);
            if(Program.udp2 != null)
                doReceive(Program.udp2);
            Program.fileList.updateComplete += updateIncomings;


            Model.SystemLog.addEntry("Launching network keep alive loops...");
            if (!isMono)
            {
                if(udtHolder != null)
                    udtHolder.launchLoop();
            }
            t = new System.Threading.Thread(transferRefreshLoop);
            t.IsBackground = true;
            t.Name = "transferRefreshLoop";
            t.Start();
            t = new System.Threading.Thread(keepAliveLoop);
            t.IsBackground = true;
            t.Name = "Reliable Keep Alive Loop";
            t.Start();
            t = new System.Threading.Thread(gossipLoop);
            t.IsBackground = true;
            t.Name = "Gossip Loop";
            t.Start();
        }
        void transferRefreshLoop()
        {
            while (!disposed)
            {
                foreach (Peer p in peerManager.allPeers)
                    if (!p.quit)
                        p.updateTransfers();
                lock (Transfer.transfers)
                    foreach (Transfer t in Transfer.transfers)
                    {
                        if (t.download == false)
                        {
                            if (t.con != null)
                            {
                                if (t.con is ReliableIncomingConnection)
                                    t.rate = ((ReliableIncomingConnection)t.con).rate;
                                
                            }
                        }
                    }
                System.Threading.Thread.Sleep(1000);
            }
        }

        void keepAliveLoop()
        {
            while (!disposed)
            {
                Commands.KeepAlive k = new Commands.KeepAlive();
                lock (incomings)
                    foreach (IncomingConnection c in incomings)
                        c.send(k);
                foreach (Peer p in peerManager.allPeers)
                {
                    if (p.udtConnection != null)
                        if (p.udtConnection.connected)
                            p.udtConnection.send(k);
                    if (p.dataConnection != null)
                        if (p.dataConnection.connected)
                            p.dataConnection.send(k);
                    if (p.controlConnection != null)
                        if (p.controlConnection.connected)
                            p.controlConnection.send(k);
                }
                System.Threading.Thread.Sleep(5000);
            }
        }
        void doReceive(System.Net.Sockets.UdpClient c)
        {
            while (!disposed)
            {
                try
                {
                    c.BeginReceive(receiveCallback, c);
                    return;
                }
                catch
                {
                    System.Threading.Thread.Sleep(10);
                }
            }
        }
        public long udpCommandsNotFromUs = 0;
        void receiveCallback(IAsyncResult ar)
        {
            System.Net.IPEndPoint sender = null;
            byte[] data = null;
            try
            {
                data = ((System.Net.Sockets.UdpClient)ar.AsyncState).EndReceive(ar, ref sender);
            }
            catch
            {
                doReceive((System.Net.Sockets.UdpClient)ar.AsyncState);
                return;
            }
            bool notFromUs = true;
            foreach (System.Net.IPAddress a in internalIPs)
                if (sender.Address.ToString() == a.ToString())
                    notFromUs = false;
            if (Program.bootstrap.publicControlEndPoint.Address.ToString() == sender.Address.ToString())
                notFromUs = false;
            if (notFromUs)
                udpCommandsNotFromUs++;

            try
            {
                lock (incomingTraffic)
                {
                    string t = Program.serializer.getType(data);
                    if (!incomingTraffic.ContainsKey(t))
                        incomingTraffic[t] = 0;
                    incomingTraffic[t] += (ulong)data.Length;
                }
            }
            catch
            {
                //whatever
            }

            Program.globalDownCounter.addBytes(data.Length);
            if (data.Length > 32 && Program.theCore != null) //Ignore extraneous STUN info
                parse(Program.serializer.deserialize(data), sender, Program.serializer.getText(data));
            doReceive((System.Net.Sockets.UdpClient)ar.AsyncState);
        }
        List<System.Net.IPEndPoint> toHello = new List<System.Net.IPEndPoint>();
        public void addPeer(System.Net.IPEndPoint p)
        {
            lock (toHello)
            {
                foreach (var v in toHello)
                    if (v.Address.ToString() == p.Address.ToString() && v.Port == p.Port)
                        return;
                toHello.Add(p);
            }
        }


        public Dictionary<string, ulong> incomingTraffic = new Dictionary<string, ulong>();
        public Dictionary<string, ulong> outgoingTraffic = new Dictionary<string, ulong>();
        
        Dictionary<string, int> knownHashes = new Dictionary<string, int>();
        Dictionary<string, List<int>> requestedHashes = new  Dictionary<string, List<int>>();
        void parse(Commands.Command c, System.Net.IPEndPoint sender, string originalText)
        {
            if (c is Commands.MiniHello)
            {
                int hash = ((Commands.MiniHello)c).helloHash;
                bool request = false;
                if (!knownHashes.ContainsKey(sender.Address.ToString() + "\n" + sender.Port.ToString()))
                    request = true;
                else
                    if (knownHashes[sender.Address.ToString() + "\n" + sender.Port.ToString()] != hash)
                    request = true;

                if (((Commands.MiniHello)c).unknown == null)
                    request = true;
                if (((Commands.MiniHello)c).unknown.HasValue)
                    if (((Commands.MiniHello)c).unknown.Value == true)
                        request = true;
                if (peerManager.parseMiniHello(((Commands.MiniHello)c), sender))
                    request = true;

                bool knownPeer = false;
                foreach (Peer p in peerManager.allPeers)
                    if(!p.quit && !p.maybeDead)
                        if (p.publicAddress.ToString() == sender.Address.ToString())
                            knownPeer = true;
                        else
                            if(p.internalAddress != null)
                                if(p.internalAddress[0].ToString() == sender.Address.ToString())
                                    knownPeer = true;
                lock (requestedHashes)
                {
                    string ip = sender.Address.ToString() + "\n" + sender.Port.ToString();
                    if (!requestedHashes.ContainsKey(ip))
                        requestedHashes[ip] = new List<int>();
                    if (!requestedHashes[ip].Contains(helloHash))   //Brand new hash code, wipe out all other ones in case we go back
                        requestedHashes[ip].Clear();
                    if (request && (!knownPeer || !requestedHashes[ip].Contains(helloHash)))
                    {
                        requestedHashes[ip].Add(((Commands.MiniHello)c).helloHash);
                        var h = generateHello();
                        h.requestingHelloBack = true;
                        byte[] b = Program.serializer.serialize(h);
                        Program.udpSend(b, sender);
                    }
                }
            }
            if (c is Commands.GossipCommand)
            {
                Commands.GossipCommand g = (Commands.GossipCommand)c;
                foreach (Commands.GossipPeer p in g.peers)
                {
                    if (p.internalAddresses == null)
                        p.internalAddresses = new string[] { p.internalAddress };
                    if (!peerManager.havePeerWithAddress(p.internalAddresses, System.Net.IPAddress.Parse(p.publicAddress)))
                    {

                        Commands.MiniHello mini = new Commands.MiniHello();
                        mini.helloHash = helloHash;
                        mini.id = id;
                        byte[] m = Program.serializer.serialize(mini);
                        //send it to both, whatever
                        Program.udpSend(m, new System.Net.IPEndPoint(System.Net.IPAddress.Parse(p.publicAddress), p.publicControlPort));
                        Program.udpSend(m, new System.Net.IPEndPoint(System.Net.IPAddress.Parse(p.internalAddress), p.internalControlPort));
                    }
                }
                if (g.requestingGossipBack)
                    sendGossip(g.circleId, sender, false);
            }
            if (c is Commands.ConnectToMe)
            {
                foreach (Peer p in Program.theCore.peerManager.allPeers)
                    if (p.id == ((Commands.ConnectToMe)c).myId)
                    {
                        p.reverseConnect();
                        return;
                    }
            }
            if (c is Commands.HelloCommand)
            {
                Commands.HelloCommand h = (Commands.HelloCommand)c;

                var z = h.peerCount;
                h.peerCount = null;
                var sha = new System.Security.Cryptography.SHA512Managed();
                int helloHash = BitConverter.ToInt32(sha.ComputeHash(Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(h).Trim())), 0);
                if (!h.requestingHelloBack)
                    knownHashes[sender.Address.ToString() + "\n" + sender.Port.ToString()] = helloHash;
                /*lock (requestedHashes)
                    if(requestedHashes.ContainsKey(sender.Address.ToString() + "\n" + sender.Port.ToString()))
                        if (requestedHashes[sender.Address.ToString() + "\n" + sender.Port.ToString()].Contains(helloHash))
                            requestedHashes[sender.Address.ToString() + "\n" + sender.Port.ToString()].Remove(helloHash);*/
                h.peerCount = z;

                if (h.debugBuild.HasValue)
                    if (!h.debugBuild.Value && h.buildNumber > Program.buildNumber)
                    {

                        System.Threading.Thread t = new System.Threading.Thread(delegate ()
                        {
                            Program.checkForUpdates();
                        });
                        t.IsBackground = true;
                        t.Name = "Program update thread";
                        t.Start();
                    }
                if (h.requestingHelloBack)
                {
                    var h2 = generateHello();
                    h2.requestingHelloBack = false;
                    byte[] b = Program.serializer.serialize(h2);
                    Program.udpSend(b, sender);
                }
                peerManager.parseHello(h, sender);
                lock(toHello)
                    if (toHello.Contains(sender))
                        toHello.Remove(sender);
            }
            if (c is Commands.RoomChatCommand)
            {
                Commands.RoomChatCommand r = (Commands.RoomChatCommand)c;
                bool received = false;
                foreach (Peer p in Program.theCore.peerManager.allPeers)
                {
                    if (p.actualEndpoint.Address.ToString() == sender.Address.ToString())
                        if (r.userId.HasValue)
                        {
                            if (r.userId == p.id)
                            {
                                p.chatReceived(r);
                                received = true;
                            }
                        }
                    if(!received)
                        if (p.internalAddress != null)
                            foreach (System.Net.IPAddress ip in p.internalAddress)
                                if (ip.ToString() == sender.Address.ToString() && p.localControlPort == sender.Port)
                                {
                                    p.chatReceived(r);
                                    received = true;
                                }
                    if(received)
                        break;
                }
                if (!received)
                    Program.theCore.chatReceived(DateTime.Now.ToShortTimeString() + " (Unknown): " + r.content.Trim('\r'), r.roomId, null);

            }
            if (c is Commands.Quitting)
            {
                Commands.Quitting r = (Commands.Quitting)c;
                foreach (Peer p in Program.theCore.peerManager.allPeers)
                    if (!p.quit || DateTime.Now.Subtract(p.timeQuit).TotalSeconds >1)
                    {
                        if (p.id == r.id)
                        {
                            p.quit = true;
                            p.timeQuit = DateTime.Now;
                            Program.theCore.peerManager.doPeerRemoved(p);
                        }
                    }
            }
            }
        List<int> usedIds = new List<int>();
        public void sendChat(string content, ulong hash)
        {
            if (content.StartsWith("/"))
            {
                if (content.ToLower().StartsWith("/nick "))
                {
                    string s = content.Substring("/nick ".Length);
                    if (s.Trim() != "")
                    {
                        if (s.Length > 16)
                            s = s.Substring(0, 16);
                        Program.settings.setString("Username", s);
                        return;
                    }
                    }


                }
            Model.Commands.RoomChatCommand c = new Commands.RoomChatCommand();
            c.content = content;
            c.userId = Program.theCore.id;
            c.roomId = hash;

            //TODO: Make this more gracefully handle collisions
            Random r = new Random();
            int id = r.Next();
            while (usedIds.Contains(id))
                id = r.Next();
            c.sequenceId = id;
            usedIds.Add(id);

            foreach (Peer p in peerManager.allPeersInCircle(hash))
            {
                p.sendCommand(c);
            }
        }
        public int incomingTcpConnections = 0;
        public int incomingUdtConnections = 0;
        List<IncomingConnection> incomings = new List<IncomingConnection>();
        public void addIncomingConnection(IncomingConnection c)
        {
            if (c is ReliableIncomingConnection)
                incomingTcpConnections++;
            if (c is UdtIncomingConnection)
                incomingUdtConnections++;
            lock (incomings)
                incomings.Add(c);
            c.commandReceived += commandReceived;
        }
        public void removeIncomingConnection(IncomingConnection c)
        {
            lock (incomings)
                incomings.Remove(c);
            c.commandReceived -= commandReceived;
        }
        List<string> toCancel = new List<string>();
        void commandReceived(Commands.Command c, IncomingConnection con)
        {
            if (con is LoopbackIncomingConnection && con.hello == null)
                con.hello = Program.theCore.generateHello();
            lock (con)
            {
                if (c is Commands.PrivateChatCommand)
                {
                    if (con.hello != null)
                    {
                        foreach (Peer p in peerManager.allPeers)
                            if (p.id == con.hello.id)
                                p.commandReceived(c);
                    }
                }
                if (c is Commands.CancelCommand)
                {
                    toCancel.Add(((Commands.CancelCommand)c).path);
                }
                if (c is Commands.GetFileListing)
                {
                    con.lastFolder = ((Commands.GetFileListing)c).path;
                    con.send(generateFileListing(((Commands.GetFileListing)c).path));
                }
                if (c is Commands.RequestChunks || c is Commands.RequestFolderContents)
                {
                    string path = "";
                    if (c is Commands.RequestChunks)
                        path = ((Commands.RequestChunks)c).path;
                    if (c is Commands.RequestFolderContents)
                        path = ((Commands.RequestFolderContents)c).path;
                    FSListing f = Program.fileList.getFSListing(path, false);
                    FSListing parent = f;
                    string fullPath = "";

                    while (parent != null)
                    {
                        FSListing p = Program.fileList.getFolder(parent.parentId);
                        if (p == null)
                        {
                            p = Program.fileList.getRootShare(parent.id);
                            if (((RootShare)p).fullPath.StartsWith("//"))
                                fullPath = "\\\\" + ((RootShare)p).fullPath.Substring(2).Replace('/', '\\') + "\\" + fullPath.Replace('/', '\\');
                            else
                                fullPath = ((RootShare)p).fullPath + "/" + fullPath;
                            fullPath = fullPath.Trim('/').TrimEnd('\\');
                            System.Threading.Thread t = new System.Threading.Thread(delegate ()
                            {
                                if (c is Commands.RequestChunks)
                                    sendCompleteFile(fullPath, path, path, con, ((Commands.RequestChunks)c).startingByte);
                                if (c is Commands.RequestFolderContents)
                                    sendCompleteFolder(fullPath, path, path, con);

                                if (toCancel.Contains(path))
                                    toCancel.Remove(path);
                            });
                            t.Name = "Upload thread";
                            t.IsBackground = true;
                            t.Start();
                            return;
                        }
                        else
                        {
                            fullPath = parent.name + "/" + fullPath;
                            parent = p;
                        }
                    }
                }
            }
        }
        void sendCompleteFolder(string realPath, string requestPath, string originalPath, IncomingConnection con)
        {
            System.IO.DirectoryInfo f = new System.IO.DirectoryInfo(realPath);
            foreach (System.IO.DirectoryInfo z in f.GetDirectories())
            {
                if (toCancel.Contains(originalPath))
                    return;
                sendCompleteFolder(z.FullName, requestPath.TrimEnd('/') + "/" + z.Name, originalPath, con);
            }
            foreach (System.IO.FileInfo z in f.GetFiles())
            {
                if (toCancel.Contains(originalPath))
                    return;
                sendCompleteFile(z.FullName, requestPath.TrimEnd('/') + "/" + z.Name, originalPath, con, 0);
            }

        }
        void sendCompleteFile(string realPath, string requestPath, string originalPath, IncomingConnection con, long startingByte)
        {
            
            int chunkSize = 64 * 1024;
            long pos = startingByte;

            System.IO.FileInfo f = new System.IO.FileInfo(realPath);
            System.IO.FileStream s = new System.IO.FileStream(realPath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            s.Seek(startingByte, System.IO.SeekOrigin.Begin);
            Transfer t = new Transfer();

            if (con is LoopbackIncomingConnection)
            {
                t.username = Program.settings.getString("Username", "Username");
                t.userId = id;
            }
            else
            {
                t.username = "(Uploading)";
                t.userId = 0;
            }
            t.filename = realPath.Substring(realPath.LastIndexOf("/") + 1);
            t.download = false;
            t.completed = (ulong) startingByte;
            t.size = (ulong)f.Length;
            t.con = con;
            t.path = requestPath;
            t.originalPath = originalPath;
            if (con is LoopbackIncomingConnection)
                t.protocol = "Loopback";
            else
                if (con is UdtIncomingConnection)
                    t.protocol = "UDT";
                else
                    t.protocol = "TCP";
            lock (Transfer.transfers)
                Transfer.transfers.Add(t);

            while (pos < f.Length)
            {
                Commands.FileChunk c = new Commands.FileChunk();
                c.start = pos;
                c.originalPath = originalPath;

                lock (con)
                {
                    if (toCancel.Contains(requestPath) || toCancel.Contains(originalPath))
                    {
                        lock (Transfer.transfers)
                            Transfer.transfers.Remove(t);
                        return;
                    }
                    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                    sw.Start();
                    byte[] buffer = new byte[Math.Min(chunkSize, f.Length - pos)];
                    int x = 0;
                    while (x < buffer.Length)
                    {
                        if (!con.connected)
                        {
                            lock (Transfer.transfers)
                                Transfer.transfers.Remove(t);
                            return;
                        }
                        int w = s.Read(buffer, x, buffer.Length - x);
                        x += w;
                    }
                    c.data = buffer;
                    c.path = requestPath;

                    con.send(c);
                    sw.Stop();
                    if (con is LoopbackIncomingConnection)
                    {
                        t.rate = ((LoopbackIncomingConnection)con).upCounter.frontBuffer;
                        t.username = Program.settings.getString("Username", "Username");
                        t.userId = Program.theCore.id;
                    }
                    else
                    {
                        t.rate = (ulong)((buffer.Length) / (sw.ElapsedTicks / (double)System.Diagnostics.Stopwatch.Frequency));
                    }
                    t.completed += (ulong)buffer.Length;
                    if (con.hello != null)
                    {
                        t.username = con.hello.username;    //TODO: Get latest username
                        t.userId = con.hello.id;
                    }
                    pos += buffer.Length;
                }
            }
            s.Dispose();


            lock (Transfer.transfers)
                Transfer.transfers.Remove(t);
            
        }
        Commands.FileListing generateFileListing(string path)
        {
            Commands.FileListing output = new Commands.FileListing();
            output.path = path;

            if (path == "/")
            {
                RootShare[] m = Program.fileListDatabase.getRootShares();

                Dictionary<string, ulong> sizes = new Dictionary<string, ulong>();
                for (int i = 0; i < m.Length; i++)
                    if (m[i] != null)
                    {
                        if (sizes.ContainsKey(m[i].name))
                            sizes[m[i].name] += m[i].size;
                        else
                            sizes[m[i].name] = m[i].size;
                    }
                output.folders = new Commands.FSListing[sizes.Count];
                int z = 0;
                foreach (string s in sizes.Keys) {
                    output.folders[z] = new Commands.FSListing() { isFolder = true, name = s, size = sizes[s] };
                    z++;
                }
            }
            else
            {
                Folder q = (Folder)Program.fileList.getFSListing(path, true);
                if (q is Folder)
                {
                    Folder f = (Folder)q;
                    List<Commands.FSListing> folders = new List<Commands.FSListing>();
                    for (int i = 0; i < f.folderIds.Length; i++)
                    {
                        FSListing z = Program.fileList.getFolder(f.folderIds[i]);
                        if(z != null)
                            folders.Add(new Commands.FSListing() { isFolder = true, name = z.name, size = z.size });
                    }
                    output.folders = folders.ToArray();

                   List <Commands.FSListing> files = new List<Commands.FSListing>();
                    for (int i = 0; i < f.fileIds.Length; i++)
                    {
                        FSListing z = Program.fileList.getFile(f.fileIds[i]);
                        if (z != null)
                            files.Add(new Commands.FSListing() { isFolder = false, name = z.name, size = z.size });
                    }
                    output.files = files.ToArray();

                }
            }
            return output;
        }
        public delegate void ChatReceivedEvent(string s, ulong id, Peer p);
        public event ChatReceivedEvent chatReceivedEvent;
        public void chatReceived(string s, ulong id, Peer p)
        {
            chatReceivedEvent?.Invoke(s, id, p);
        }

        internal struct LASTINPUTINFO
        {
            public uint cbSize;

            public uint dwTime;
        }
        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool
                GetLastInputInfo(ref LASTINPUTINFO plii);

        public static uint getIdleTime()
        {
            LASTINPUTINFO lastInput = new LASTINPUTINFO();
            lastInput.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInput);
            GetLastInputInfo(ref lastInput);

            return (uint)Environment.TickCount - lastInput.dwTime;
        }
        public System.Net.IPAddress[] internalIPs = new System.Net.IPAddress[] { };
        int helloHash = 0;
        public Commands.HelloCommand generateHello()
        {
            Commands.HelloCommand c = new Commands.HelloCommand();
            c.id = id;
            c.username = Program.settings.getString("Username", "Username");
            if (isMono)
                Program.settings.setBool("Use UDT", false);

            c.useUDT = Program.settings.getBool("Use UDT", true);
            c.description = Program.settings.getString("Description", "");

            try
            {
                c.afk = getIdleTime() > 60 * 1000;
            }
            catch
            {
                c.afk = false;
            }
            if (!Program.settings.getBool("Show AFK", true))
                c.afk = false;

            Dictionary<ulong, int> counts = new Dictionary<ulong, int>();
            foreach (Peer p in peerManager.allPeers)
            {
                if (!p.maybeDead)
                {
                    foreach (ulong i in p.circles)
                        if (!counts.ContainsKey(i))
                            counts[i] = 1;
                        else
                            counts[i]++;
                }
            }
#if DEBUG
            c.debugBuild = true;
#else
            c.debugBuild  = false;
#endif
            c.peerCount = counts;
            if (Program.bootstrap.publicControlEndPoint != null)
            {
                c.externalIP = Program.bootstrap.publicControlEndPoint.Address.ToString();
                c.externalControlPort = Program.bootstrap.publicControlEndPoint.Port;
            }
            if (Program.bootstrap.publicDataEndPoint != null)
            {
                c.externalDataPort = Program.bootstrap.publicDataEndPoint.Port;
            }
            c.internalControlPort = Program.bootstrap.internalControlPort;
            c.internalDataPort = Program.bootstrap.internalDataPort;
            if (isMono)
            {
                c.useUDT = false;
                c.internalUdtPort = 0;
            }
            else
            {
                if (udtHolder != null)
                    c.internalUdtPort = udtHolder.listenPort;
                else
                    c.internalUdtPort = 0;
            }
            c.buildNumber = Program.buildNumber;
            c.behindDoubleNAT = Program.bootstrap.behindDoubleNAT;

            System.Security.Cryptography.SHA512Managed sha = new System.Security.Cryptography.SHA512Managed();
            List<ulong> circles = new List<ulong>();
            foreach (string s in this.circles)
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(s.ToLower()));
                circles.Add(BitConverter.ToUInt64(hash, 0));
                
            }
            c.myCircles = circles.ToArray();

            ulong share = 0;
            foreach (RootShare r in Program.fileListDatabase.getRootShares())
                if (r != null)
                    share += r.size;
            c.myShare = share;

            //too much output!
            var n = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
            List<string> ips = new List<string>();
            List<System.Net.IPAddress> ips2 = new List<System.Net.IPAddress>();
            for (int i = 0; i < n.Length; i++)
                foreach (var ni in n[i].GetIPProperties().UnicastAddresses)
                    if (ni.Address.ToString() != System.Net.IPAddress.Loopback.ToString() && ni.Address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
                        if (ni.IsDnsEligible)
                        {
                            ips2.Add(ni.Address);
                            ips.Add(ni.Address.ToString());
                        }
            if (ips.Count == 0)
            {
                for (int i = 0; i < n.Length; i++)
                    foreach (var ni in n[i].GetIPProperties().UnicastAddresses)
                        if (ni.Address.ToString() != System.Net.IPAddress.Loopback.ToString() && ni.Address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
                        {
                            ips2.Add(ni.Address);
                            ips.Add(ni.Address.ToString());
                        }
            }
            c.internalIPs = ips.ToArray();
            internalIPs = ips2.ToArray();

            var z = c.peerCount;
            c.peerCount = null;
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(c);
            helloHash = BitConverter.ToInt32(sha.ComputeHash(Encoding.UTF8.GetBytes(json.Trim())),0);
            c.peerCount = z;
            return c;
        }
        void helloLoop()
        {
            while (!disposed)
            {
                while (Program.theCore == null && !disposed)
                    System.Threading.Thread.Sleep(10);
                while (!Program.doneLoading && !disposed)
                    System.Threading.Thread.Sleep(10);
                if (disposed)
                    return;
                

                Commands.MiniHello mini = new Commands.MiniHello();
                mini.helloHash = helloHash;
                mini.id = id;
                mini.unknown = false;
                byte[] m = Program.serializer.serialize(mini);
                mini.unknown = true;
                byte[] m2 = Program.serializer.serialize(mini);
                
                //Program.udpSend(b, b.Length, new System.Net.IPEndPoint(System.Net.IPAddress.Broadcast, NetConstants.controlPort));
                Program.udpSend(m, m.Length, new System.Net.IPEndPoint(System.Net.IPAddress.Broadcast, NetConstants.controlPort));
                

                if (((System.Net.IPEndPoint)Program.udp.Client.LocalEndPoint).Port != NetConstants.controlPort)
                {
                    //Program.udpSend(b, b.Length, new System.Net.IPEndPoint(System.Net.IPAddress.Broadcast, ((System.Net.IPEndPoint)Program.udp.Client.LocalEndPoint).Port));
                    Program.udpSend(m, m.Length, new System.Net.IPEndPoint(System.Net.IPAddress.Broadcast, ((System.Net.IPEndPoint)Program.udp.Client.LocalEndPoint).Port));
                }
                System.Threading.Thread.Sleep(1000);
                if (disposed)
                    return;

                lock (toHello)
                    foreach (System.Net.IPEndPoint p in toHello)
                    {
                        foreach (Peer p2 in peerManager.allPeers)
                            if(p2.internalAddress != null)
                                if (p2.internalAddress.ToString() == p.Address.ToString() || p2.publicAddress.ToString() == p.Address.ToString())
                                    continue;
                        if (p.Address.ToString() != Program.bootstrap.publicControlEndPoint.Address.ToString())
                        {
                            try
                            {
                                //Program.udpSend(b, b.Length, p);
                                Program.udpSend(m2, m2.Length, p);
                            }
                            catch
                            {
                                //probably invalid IP, ignore
                            }
                        }
                    }
                System.Threading.Thread.Sleep(1000);
                if (disposed)
                    return;
                foreach (Peer p in peerManager.allPeers)
                {
                    //if (DateTime.Now.Subtract(p.lastTimeHelloSent).TotalSeconds > 30 || lastHelloHash != helloHash)
                    {
                        //if (p.id != Program.theCore.id)
                        {
                            //if (!p.quit)
                            {
                                //Program.udpSend(b, b.Length, p.actualEndpoint);
                                //Program.globalUpCounter.addBytes(b.Length);
                                try
                                {

                                    Program.udpSend(m2, p.actualEndpoint);
                                    if (p.maybeDead)
                                    {
                                        if (p.isLocal)
                                        {
                                            if (p.internalAddress[0].ToString() != p.actualEndpoint.Address.ToString())
                                                Program.udpSend(m2, new System.Net.IPEndPoint(p.internalAddress[0], p.localControlPort));
                                        }
                                        else
                                        {
                                            if (p.publicAddress.ToString() != p.actualEndpoint.Address.ToString())
                                                Program.udpSend(m2, new System.Net.IPEndPoint(p.publicAddress, p.externalControlPort));
                                        }
                                    }
                                    else
                                    {
                                        if (p.isLocal)
                                        {
                                            if (p.internalAddress[0].ToString() != p.actualEndpoint.Address.ToString())
                                                Program.udpSend(m, new System.Net.IPEndPoint(p.internalAddress[0], p.localControlPort));
                                        }
                                        else
                                        {
                                            if (p.publicAddress.ToString() != p.actualEndpoint.Address.ToString())
                                                Program.udpSend(m, new System.Net.IPEndPoint(p.publicAddress, p.externalControlPort));
                                        }
                                    }
                                }
                                catch
                                {
                                    //probably invalid IP, ignore
                                }
                                p.lastTimeHelloSent = DateTime.Now;
                            }
                        }
                    }
                }
                iteration++;
                if (iteration >= 10)
                    iteration = 0;
                System.Threading.Thread.Sleep(1000);
                if (disposed)
                    return;
            }
        }
        int iteration = 0;
    }
}
