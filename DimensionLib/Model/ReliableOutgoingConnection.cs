﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    public class ReliableOutgoingConnection : OutgoingConnection
    {
        public static int successfulConnections = 0;
        public override event CommandReceived commandReceived;
        public ReliableOutgoingConnection(System.Net.Sockets.TcpClient client)
        {
            this.client = client;
            send(App.theCore.generateHello());
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

            send(App.theCore.generateHello());
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
                    while (pos < 4)
                    {
                        read = client.GetStream().Read(lenByte, pos, 4-pos);
                        pos += read;
                    }
                    App.globalDownCounter.addBytes((ulong)4);
                    dataByte = new byte[BitConverter.ToInt32(lenByte, 0)];

                    if (dataByte.Length == 0)
                        return;
                    pos = 0;
                    read = 1;
                    while (pos < dataByte.Length)
                    {
                        read = client.GetStream().Read(dataByte, pos, dataByte.Length - pos);
                        App.globalDownCounter.addBytes((ulong)read);
                        pos += read;
                    }
                    
                }
                catch
                {
                    return;
                }
                c = App.serializer.deserialize(dataByte);
                try
                {
                    if (c is Commands.DataCommand)
                    {
                        int pos = 0;
                        int read = 1;
                        byte[] chunk = new byte[((Commands.DataCommand)c).dataLength];
                        while (pos < chunk.Length)
                        {
                            int amt = (int)App.speedLimiter.limitDownload((ulong)(chunk.Length - pos), rateLimiterDisabled);
                            read = client.GetStream().Read(chunk, pos,amt);
                            downCounter.addBytes(read);
                            currentRate = (currentRate * 0.9f) + (downCounter.frontBuffer * 0.1f);
                            rate = (ulong)currentRate;
                            App.globalDownCounter.addBytes((ulong)read);
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
            byte[] b = App.serializer.serialize(c);
            int len = b.Length;
            lock (sendLock)
            {
                try
                {
                    client.GetStream().Write(BitConverter.GetBytes(len), 0, 4);
                    App.globalUpCounter.addBytes((ulong)4);
                    client.GetStream().Write(b, 0, b.Length);
                    App.globalUpCounter.addBytes((ulong)b.Length);
                    if (c is Commands.DataCommand)
                    {
                        int pos = 0;
                        while (pos < ((Commands.DataCommand)c).data.Length)
                        {
                            int amt = (int)App.speedLimiter.limitUpload((ulong)(((Commands.DataCommand)c).data.Length - pos), rateLimiterDisabled);
                            client.GetStream().Write(((Commands.DataCommand)c).data, pos, amt);
                            pos += amt;
                            App.globalUpCounter.addBytes(amt);
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

