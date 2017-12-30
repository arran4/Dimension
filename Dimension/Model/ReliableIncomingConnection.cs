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
                byte[] lenByte = new byte[4];
                client.GetStream().Read(lenByte, 0, 4);
                byte[] dataByte = new byte[BitConverter.ToInt32(lenByte, 0)];

                int pos = 0;
                int read = 1;
                while (read > 0 && pos < dataByte.Length)
                {
                    read = client.GetStream().Read(dataByte, pos, dataByte.Length - pos);
                    pos += read;
                }
                Commands.Command c = Program.serializer.deserialize(dataByte);
                if (c is Commands.DataCommand)
                {
                    pos = 0;
                    read = 1;
                    byte[] chunk = new byte[((Commands.DataCommand)c).dataLength];
                    while (read > 0 && pos < chunk.Length)
                    {
                        read = client.GetStream().Read(chunk, pos, chunk.Length - pos);
                        pos += read;
                    }
                    ((Commands.DataCommand)c).data = chunk;
                }
                commandReceived?.Invoke(c, this);
            }
        }
        
        System.Net.Sockets.TcpClient client;
        object sendLock = new object();
        public override void send(Commands.Command c)
        {
            byte[] b = Program.serializer.serialize(c);
            int len = b.Length;
            lock (sendLock)
            {
                if (c is Commands.DataCommand)
                    ((Commands.DataCommand)c).dataLength = ((Commands.DataCommand)c).data.Length;
                client.GetStream().Write(BitConverter.GetBytes(len), 0, 4);
                client.GetStream().Write(b, 0, b.Length);
                if (c is Commands.DataCommand)
                    client.GetStream().Write(((Commands.DataCommand)c).data, 0, ((Commands.DataCommand)c).data.Length);
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
