﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    class UdtIncomingConnection : IncomingConnection
    {
        public override event CommandReceived commandReceived;
        public UdtIncomingConnection(Udt.Socket s)
        {
            socket = s;
            System.Threading.Thread t = new System.Threading.Thread(receiveLoop);
            t.IsBackground = true;
            t.Name = "UDT receive loop";
            t.Start();
        }
        void receiveLoop()
        {
            while (isConnected)
            {
                byte[] lenByte = new byte[4];
                socket.Receive(lenByte);
                byte[] dataByte = new byte[BitConverter.ToInt32(lenByte, 0)];
                socket.Receive(dataByte);

                commandReceived?.Invoke(Program.serializer.deserialize(dataByte), this);
            }
        }
        bool isConnected = true;
        Udt.Socket socket;
        public override void send(Commands.Command c)
        {
            byte[] b = Program.serializer.serialize(c);
            int len = b.Length;
            socket.Send(BitConverter.GetBytes(len), 0, 4);
            socket.Send(b, 0, b.Length);
        }
        public override bool connected
        {
            get
            {
                if (socket.State == Udt.SocketState.Closed || socket.State == Udt.SocketState.Closing)
                    isConnected = false;
                return isConnected;
            }
        }
    }
}
