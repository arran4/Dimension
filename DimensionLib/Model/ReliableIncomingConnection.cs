﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    public class ReliableIncomingConnection : IncomingConnection
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
                    while (pos < 4)
                    {
                        read = client.GetStream().Read(lenByte, pos, 4 - pos);
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
                            read = client.GetStream().Read(chunk, pos, (int)App.speedLimiter.limitDownload((ulong)(chunk.Length - pos), rateLimiterDisabled));
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
                if (c is Commands.ReverseConnectionType)
                {
                    Commands.ReverseConnectionType r = (Commands.ReverseConnectionType)c;
                    foreach (Peer p in App.theCore.peerManager.allPeers)
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
                            App.theCore.removeIncomingConnection(this);
                            return;
                        }

                        }
                commandReceived?.Invoke(c, this);
                if (c is Commands.HelloCommand)
                    hello = (Commands.HelloCommand)c;
            }
        }
        public ulong rate;
        public ByteCounter rateCounter = new ByteCounter();
        float internalRate;
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
                            rateCounter.addBytes(amt);
                            internalRate = (internalRate * 0.9f) + (rateCounter.frontBuffer * 0.1f);
                            rate = (ulong)internalRate;
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
