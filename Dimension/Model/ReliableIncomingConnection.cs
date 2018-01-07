using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    class ReliableIncomingConnection : IncomingConnection
    {
        public override event CommandReceived commandReceived;
        public ReliableIncomingConnection(System.Net.Sockets.TcpClient c)
        {
            client = c;
            System.Threading.Thread t = new System.Threading.Thread(receiveLoop);
            t.IsBackground = true;
            t.Name = "UDT receive loop";
            t.Start();
        }

        void receiveLoop()
        {
            while (connected)
            {
                byte[] dataByte;
                Commands.Command c;
                try
                {
                    byte[] lenByte = new byte[4];

                    int pos = 0;
                    int read = 1;
                    while (pos < 4 && read > 0)
                    {
                        read = client.GetStream().Read(lenByte, pos, 4 - pos);
                        pos += read;
                    }
                    Program.globalDownCounter.addBytes((ulong)4);
                    dataByte = new byte[BitConverter.ToInt32(lenByte, 0)];

                    if (dataByte.Length == 0)
                        return;
                    pos = 0;
                    read = 1;
                    while (read > 0 && pos < dataByte.Length)
                    {
                        read = client.GetStream().Read(dataByte, pos, dataByte.Length - pos);
                        Program.globalDownCounter.addBytes((ulong)read);
                        pos += read;
                    }

                }
                catch
                {
                    return;
                }
                c = Program.serializer.deserialize(dataByte);
                try
                {
                    if (c is Commands.DataCommand)
                    {
                        int pos = 0;
                        int read = 1;

                        Program.speedLimiter.limitDownload((ulong)((Commands.DataCommand)c).data.Length);
                        byte[] chunk = new byte[((Commands.DataCommand)c).dataLength];
                        while (read > 0 && pos < chunk.Length)
                        {
                            read = client.GetStream().Read(chunk, pos, chunk.Length - pos);
                            Program.globalDownCounter.addBytes((ulong)read);
                            pos += read;
                        }
                        ((Commands.DataCommand)c).data = chunk;
                    }
                }
                catch
                {
                    return;
                }
                if (c is Commands.ReverseConnectionType)
                {
                    Commands.ReverseConnectionType r = (Commands.ReverseConnectionType)c;
                    foreach (Peer p in Program.theCore.peerManager.allPeers)
                        if (p.id == r.id)
                        {
                            if (r.makeControl)
                            {
                                p.controlConnection = new ReliableOutgoingConnection(client);
                                p.controlConnection.commandReceived += p.commandReceived;
                            }
                            if (r.makeData)
                            {
                                p.dataConnection = new ReliableOutgoingConnection(client);
                                p.dataConnection.commandReceived += p.commandReceived;
                            }
                            Program.theCore.removeIncomingConnection(this);
                            return;
                        }

                        }
                commandReceived?.Invoke(c, this);
                if (c is Commands.HelloCommand)
                    hello = (Commands.HelloCommand)c;
            }
        }
        System.Net.Sockets.TcpClient client;
        object sendLock = new object();
        public override void send(Commands.Command c)
        {
            if (c is Commands.DataCommand)
                ((Commands.DataCommand)c).dataLength = ((Commands.DataCommand)c).data.Length;
            byte[] b = Program.serializer.serialize(c);
            int len = b.Length;
            lock (sendLock)
            {
                try
                {
                    client.GetStream().Write(BitConverter.GetBytes(len), 0, 4);
                    Program.globalUpCounter.addBytes((ulong)4);
                    client.GetStream().Write(b, 0, b.Length);
                    Program.globalUpCounter.addBytes((ulong)b.Length);
                    if (c is Commands.DataCommand)
                    {
                        Program.speedLimiter.limitUpload((ulong)((Commands.DataCommand)c).data.Length);
                        client.GetStream().Write(((Commands.DataCommand)c).data, 0, ((Commands.DataCommand)c).data.Length);
                        Program.globalUpCounter.addBytes((ulong)((Commands.DataCommand)c).data.Length);
                    }
                }

                catch
                {
                    return;
                }
            }
        }
        public override bool connected
        {
            get
            {
                return client.Connected;
            }
        }
    }
}
