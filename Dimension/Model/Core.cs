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
        ulong id;
        public Core()
        {
            Random r = new Random();
            ulong randomId = (ulong)r.Next();
            randomId = randomId << 32;
            randomId |= (uint)r.Next();

            id = (ulong)Program.settings.getULong("ID", randomId);
            peerManager = new PeerManager();
            System.Threading.Thread t = new System.Threading.Thread(helloLoop);
            t.IsBackground = true;
            t.Name = "Hello send loop";
            t.Start();
            doReceive();
        }

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
