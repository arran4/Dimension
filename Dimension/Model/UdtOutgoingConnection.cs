using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    class UdtOutgoingConnection : OutgoingConnection
    {
        public override event CommandReceived commandReceived;
        public UdtOutgoingConnection(System.Net.IPAddress addr, int port)
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
                byte[] lenByte = new byte[4];
                client.GetStream().Read(lenByte, 0, 4);
                byte[] dataByte = new byte[BitConverter.ToInt32(lenByte, 0)];

                int pos = 0;
                int read = 1;
                while (read > 0 && pos < dataByte.Length)
                {
                    read = client.GetStream().Read(dataByte, pos, dataByte.Length-pos);
                    pos += read;
                }
                commandReceived?.Invoke(Program.serializer.deserialize(dataByte));
            }
        }
        object sendLock = new object();
        System.Net.Sockets.TcpClient client;
        public override void send(Commands.Command c)
        {
            byte[] b = Program.serializer.serialize(c);
            int len = b.Length;
            lock (sendLock)
            {
                client.GetStream().Write(BitConverter.GetBytes(len), 0, 4);
                
                client.GetStream().Write(b, 0, b.Length);
                
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

