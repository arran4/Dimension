using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    class ReliableOutgoingConnection : OutgoingConnection
    {
        public override event CommandReceived commandReceived;
        public ReliableOutgoingConnection(System.Net.IPAddress addr, int port)
        {
            client = new System.Net.Sockets.TcpClient();
            client.Connect(addr, port);
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
                    client.GetStream().Read(lenByte, 0, 4);
                    dataByte = new byte[BitConverter.ToInt32(lenByte, 0)];

                    if (dataByte.Length == 0)
                        return;
                    int pos = 0;
                    int read = 1;
                    while (read > 0 && pos < dataByte.Length)
                    {
                        read = client.GetStream().Read(dataByte, pos, dataByte.Length - pos);
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
                        byte[] chunk = new byte[((Commands.DataCommand)c).dataLength];
                        while (read > 0 && pos < chunk.Length)
                        {
                            read = client.GetStream().Read(chunk, pos, chunk.Length - pos);
                            pos += read;
                        }
                        ((Commands.DataCommand)c).data = chunk;
                    }
                }
                catch
                {
                    return;
                }
                commandReceived?.Invoke(c);
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
                    client.GetStream().Write(b, 0, b.Length);
                    if (c is Commands.DataCommand)
                        client.GetStream().Write(((Commands.DataCommand)c).data, 0, ((Commands.DataCommand)c).data.Length);
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

