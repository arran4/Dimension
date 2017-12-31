﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    class UdtOutgoingConnection : OutgoingConnection
    {
        Udt.Socket socket;
        public override event CommandReceived commandReceived;
        public UdtOutgoingConnection(System.Net.IPAddress addr, int port)
        {
            socket= new Udt.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream);
            socket.Connect(addr, port);

            System.Threading.Thread t = new System.Threading.Thread(receiveLoop);
            t.IsBackground = true;
            t.Name = "UDT receive loop";
            t.Start();
        }
        void receiveLoop()
        {
            while (connected)
            {
                byte[] dataByte = null;
                int pos = 0;
                int read = 1;
                try
                {
                    byte[] lenByte = new byte[4];
                    socket.Receive(lenByte);
                    dataByte = new byte[BitConverter.ToInt32(lenByte, 0)];
                    
                    if (dataByte.Length == 0)
                        return;

                    while (read > 0 && pos < dataByte.Length)
                    {
                        read = socket.Receive(dataByte, pos, dataByte.Length - pos);
                        pos += read;
                    }
                }
                catch
                {
                    return;
                }
                Commands.Command c = Program.serializer.deserialize(dataByte);
                if (c is Commands.DataCommand)
                {
                    pos = 0;
                    read = 1;
                    byte[] chunk = new byte[((Commands.DataCommand)c).dataLength];
                    while (read > 0 && pos < chunk.Length)
                    {
                        read = socket.Receive(chunk, pos, chunk.Length - pos);
                        pos += read;
                    }
                    ((Commands.DataCommand)c).data = chunk;
                }
                commandReceived?.Invoke(c);
            }
        }
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
                    socket.Send(BitConverter.GetBytes(len));
                    
                    int pos = 0;
                    int read = 1;
                    while (read > 0 && pos < b.Length)
                    {
                        read = socket.Send(b, pos, b.Length - pos);
                        pos += read;
                    }
                    if (c is Commands.DataCommand)
                    {
                        b = ((Commands.DataCommand)c).data;
                        pos = 0;
                        read = 1;
                        while (read > 0 && pos < b.Length)
                        {
                            read = socket.Send(b, pos, b.Length - pos);
                            pos += read;
                        }
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
                return socket.State != Udt.SocketState.Closed;
            }
        }
    }
}