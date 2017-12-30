﻿using System;
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
        bool disposed = false;
        public void Dispose()
        {
            disposed = true;

        }
        public void joinCircle(string s)
        {
            lock (circles)
                circles.Add(s);
        }
        void updateIncomings()
        {
            lock(incomings)
                foreach(IncomingConnection z in incomings)
                    if(z.lastFolder != null)
                        z.send(generateFileListing(z.lastFolder));

        }
        public ulong id;
        public Core()
        {
            Random r = new Random();
            ulong randomId = (ulong)r.Next();
            randomId = randomId << 32;
            randomId |= (uint)r.Next();

            udtListener = new Udt.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream);
            udtListener.Bind(System.Net.IPAddress.Any, 0);
            udtListener.Listen(1000);

            id = (ulong)Program.settings.getULong("ID", randomId);
            peerManager = new PeerManager();
            System.Threading.Thread t = new System.Threading.Thread(helloLoop);
            t.IsBackground = true;
            t.Name = "Hello send loop";
            t.Start();
            doReceive();
            Program.fileList.updateComplete += updateIncomings;

            t = new System.Threading.Thread(udtAcceptLoop);
            t.IsBackground = true;
            t.Name = "UDT Accept Loop";
            t.Start();
        }

        void udtAcceptLoop()
        {
            while (!disposed)
            {
                Udt.Socket s = udtListener.Accept();
                var p = new UdtIncomingConnection(s);
                addIncomingConnection(p);
            }


            }
        Udt.Socket udtListener;
        void doReceive()
        {
            while (!disposed)
            {
                try
                {
                    Program.udp.BeginReceive(receiveCallback, null);
                    return;
                }
                catch
                {
                    System.Threading.Thread.Sleep(10);
                }
            }
        }
        void receiveCallback(IAsyncResult ar)
        {
            System.Net.IPEndPoint sender = null;
            byte[] data = null;
            try
            {
                data = Program.udp.EndReceive(ar, ref sender);
            }
            catch
            {
                doReceive();
                return;
            }
            if(data.Length > 32) //Ignore extraneous STUN info
                parse(Program.serializer.deserialize(data), sender);
            doReceive();
        }
        void parse(Commands.Command c, System.Net.IPEndPoint sender)
        {
            if (c is Commands.HelloCommand)
            {
                Commands.HelloCommand h = (Commands.HelloCommand)c;
                peerManager.parseHello(h, sender);

            }
            if (c is Commands.RoomChatCommand)
            {
                Commands.RoomChatCommand r = (Commands.RoomChatCommand)c;
                foreach (Peer p in Program.theCore.peerManager.allPeers)
                    if (p.actualEndpoint.ToString() == sender.ToString())
                        p.chatReceived(r);

            }
            }
        List<int> usedIds = new List<int>();
        public void sendChat(string content, ulong hash)
        {
            Model.Commands.RoomChatCommand c = new Commands.RoomChatCommand();
            c.content = content;
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
        List<IncomingConnection> incomings = new List<IncomingConnection>();
        public void addIncomingConnection(IncomingConnection c)
        {
            lock (incomings)
                incomings.Add(c);
            c.commandReceived += commandReceived;
        }
        void commandReceived(Commands.Command c, IncomingConnection con)
        {
            if (c is Commands.GetFileListing)
            {
                con.lastFolder = ((Commands.GetFileListing)c).path;
                con.send(generateFileListing(((Commands.GetFileListing)c).path));
            }
            if (c is Commands.RequestChunks)
            {
                var z = (Commands.RequestChunks)c;
                FSListing f = Program.fileList.getFSListing(z.path, false);
                FSListing parent = f;
                string fullPath = "";

                while (parent != null)
                {
                    FSListing p = Program.fileList.getFolder(parent.parentId);
                    if (p == null)
                    {
                        p = Program.fileList.getRootShare(parent.id);
                        fullPath = ((RootShare)p).fullPath + "/" + fullPath;

                        fullPath = fullPath.Trim('/');
                        sendCompleteFile(fullPath, z.path, con);
                    }
                    else
                    {
                        fullPath = parent.name + "/" + fullPath;
                        parent = p;
                    }
                }
            }
            }
        void sendCompleteFile(string realPath, string requestPath, IncomingConnection con)
        {
            int chunkSize = 64 * 1024;
            int pos = 0;

            System.IO.FileInfo f = new System.IO.FileInfo(realPath);
            System.IO.FileStream s = new System.IO.FileStream(realPath, System.IO.FileMode.Open, System.IO.FileAccess.Read);


            while (pos < f.Length)
            {
                Commands.FileChunk c = new Commands.FileChunk();
                c.start = pos;

                byte[] buffer = new byte[Math.Min(chunkSize, f.Length - pos)];
                int x = 0;
                while (x < buffer.Length)
                    x += s.Read(buffer, x, buffer.Length - x);

                c.data = buffer;
                c.path = requestPath;
                con.send(c);
                pos += buffer.Length;
            }
            s.Dispose();




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
                    output.folders = new Commands.FSListing[f.folderIds.Length];
                    for (int i = 0; i < f.folderIds.Length; i++)
                    {
                        FSListing z = Program.fileList.getFolder(f.folderIds[i]);
                        output.folders[i] = new Commands.FSListing() { isFolder = true, name = z.name, size = z.size };
                    }
                    output.files = new Commands.FSListing[f.fileIds.Length];

                    for (int i = 0; i < f.fileIds.Length; i++)
                    {
                        FSListing z = Program.fileList.getFile(f.fileIds[i]);
                        output.files[i] = new Commands.FSListing() { isFolder = false, name = z.name, size = z.size };
                    }

                }
            }
            return output;
        }
        public delegate void ChatReceivedEvent(string s, ulong id);
        public event ChatReceivedEvent chatReceivedEvent;
        public void chatReceived(string s, ulong id)
        {
            chatReceivedEvent?.Invoke(s, id);
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
                Commands.HelloCommand c = new Commands.HelloCommand();
                c.id = id;
                c.username = Program.settings.getString("Username", "Username");
                c.machineName = Environment.MachineName;
                Dictionary<int, int> counts = new Dictionary<int, int>();
                foreach (Peer p in Program.theCore.peerManager.allPeers)
                {
                    foreach (int i in p.circles)
                        if (!counts.ContainsKey(i))
                            counts[i] = 1;
                        else
                            counts[i]++;
                }
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
                c.internalUdtPort = udtListener.LocalEndPoint.Port;

                System.Security.Cryptography.SHA512Managed sha = new System.Security.Cryptography.SHA512Managed();
                List<ulong> circles = new List<ulong>();
                foreach (string s in this.circles)
                {
                    byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(s));
                    circles.Add(BitConverter.ToUInt64(hash, 0));
                }
                c.myCircles = circles.ToArray();

                ulong share = 0;
                foreach (RootShare r in Program.fileListDatabase.getRootShares())
                    if(r != null)
                        share += r.size;
                c.myShare = share;

                //too much output!
                /*var n = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
                List<string> ips = new List<string>();
                for (int i = 0; i < n.Length; i++)
                    foreach (var ni in n[i].GetIPProperties().UnicastAddresses)
                        ips.Add(ni.Address.ToString());
                c.internalIPs = ips.ToArray();
                */

                byte[] b = Program.serializer.serialize(c);

                Program.udp.Send(b, b.Length, new System.Net.IPEndPoint(System.Net.IPAddress.Broadcast, NetConstants.controlPort));

                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}
