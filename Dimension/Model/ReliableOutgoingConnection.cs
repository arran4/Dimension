﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    class ReliableOutgoingConnection : OutgoingConnection
    {
        public static int successfulConnections = 0;
        public override event CommandReceived commandReceived;
        public ReliableOutgoingConnection(System.Net.Sockets.TcpClient client)
        {
            this.client = client;
            send(Program.theCore.generateHello());
            System.Threading.Thread t = new System.Threading.Thread(receiveLoop);
            t.IsBackground = true;
            t.Name = "TCP receive loop";
            t.Start();
        }
        public ReliableOutgoingConnection(System.Net.IPAddress addr, int port)
        {
            client = new System.Net.Sockets.TcpClient();
            client.Connect(addr, port);
            successfulConnections++;

            send(Program.theCore.generateHello());
            System.Threading.Thread t = new System.Threading.Thread(receiveLoop);
            t.IsBackground = true;
            t.Name = "TCP receive loop";
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
                        read = client.GetStream().Read(lenByte, pos, 4-pos);
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
                        byte[] chunk = new byte[((Commands.DataCommand)c).dataLength];
                        while (read > 0 && pos < chunk.Length)
                        {
                            int amt = (int)Program.speedLimiter.limitDownload((ulong)(chunk.Length - pos));
                            read = client.GetStream().Read(chunk, pos,amt);
                            downCounter.addBytes(read);
                            currentRate = (currentRate * 0.9f) + (downCounter.frontBuffer * 0.1f);
                            rate = (ulong)currentRate;
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
                commandReceived?.Invoke(c);
            }
        }
        ByteCounter downCounter = new ByteCounter();
        float currentRate;

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
                        int pos = 0;
                        while (pos < ((Commands.DataCommand)c).data.Length)
                        {
                            int amt = (int)Program.speedLimiter.limitUpload((ulong)(((Commands.DataCommand)c).data.Length - pos));
                            client.GetStream().Write(((Commands.DataCommand)c).data, pos, amt);
                            pos += amt;
                            Program.globalUpCounter.addBytes(amt);
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
                return client.Connected;
            }
        }
    }
}

