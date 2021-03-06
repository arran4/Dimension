﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    class UdtIncomingConnection : IncomingConnection
    {
        Udt.Socket socket;
        public override event CommandReceived commandReceived;
        System.Net.Sockets.Socket underlying;
        public UdtIncomingConnection(Udt.Socket socket, System.Net.Sockets.Socket underlying)
        {
            this.underlying = underlying;
            this.socket = socket;

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
                    while (pos < lenByte.Length)
                    {
                        read = socket.Receive(lenByte, pos, lenByte.Length - pos);
                        pos += read;
                        App.globalDownCounter.addBytes((ulong)read);
                    }
                    pos = 0;
                    App.globalDownCounter.addBytes(4);
                    dataByte = new byte[BitConverter.ToInt32(lenByte, 0)];
                    
                    if (dataByte.Length > 0)
                    {
                        while (pos < dataByte.Length)
                        {
                            read = socket.Receive(dataByte, pos, dataByte.Length - pos);
                            pos += read;
                            App.globalDownCounter.addBytes((ulong)read);
                        }
                    }
                }
                catch
                {
                    continue;
                }
                Commands.Command c = App.serializer.deserialize(dataByte);
                if (c is Commands.DataCommand)
                {
                    pos = 0;
                    read = 1;
                    App.speedLimiter.limitDownload((ulong)((Commands.DataCommand)c).data.Length);
                    byte[] chunk = new byte[((Commands.DataCommand)c).dataLength];
                    while (pos < chunk.Length)
                    {
                        read = socket.Receive(chunk, pos, (int)App.speedLimiter.limitDownload((ulong)(chunk.Length - pos), rateLimiterDisabled));
                        pos += read;
                        App.globalDownCounter.addBytes((ulong)read);
                    }
                    ((Commands.DataCommand)c).data = chunk;
                }
                if (c is Commands.HelloCommand)
                    hello = (Commands.HelloCommand)c;
                while (commandReceived == null && connected)
                    System.Threading.Thread.Sleep(10);
                commandReceived?.Invoke(c, this);
            }
        }
        object sendLock = new object();
        public override void send(Commands.Command c)
        {
            if (c is Commands.DataCommand)
                ((Commands.DataCommand)c).dataLength = ((Commands.DataCommand)c).data.Length;
            byte[] b = App.serializer.serialize(c);
            int len = b.Length;
            lock (sendLock)
            {
                try
                {
                    int pos = 0;
                    int read = 1;

                    while (pos < 4)
                    {
                        read = socket.Send(BitConverter.GetBytes(len), pos, 4 - pos);
                        App.globalUpCounter.addBytes((ulong)read);
                        pos += read;
                    }
                    pos = 0;
                    App.globalUpCounter.addBytes(4);

                    while (pos < b.Length)
                    {
                        read = socket.Send(b, pos, b.Length - pos);
                        App.globalUpCounter.addBytes((ulong)read);
                        pos += read;
                    }

                    if (c is Commands.DataCommand)
                    {
                        b = ((Commands.DataCommand)c).data;
                        pos = 0;
                        read = 1;
                        while (pos < b.Length)
                        {
                            int amt = (int)App.speedLimiter.limitUpload((ulong)(b.Length - pos), rateLimiterDisabled);
                            read = socket.Send(b, pos, amt);
                            App.globalUpCounter.addBytes((ulong)read);
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
        public bool connecting
        {
            get
            {

                try
                {
                    return socket.State == Udt.SocketState.Connecting;
                }
                catch
                {
                    return false;
                }
            }
        }
        public override bool connected
        {
            get
            {
                try
                {
                    return socket.State != Udt.SocketState.Closed;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
