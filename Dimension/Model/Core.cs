using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    public class Core : IDisposable
    {
        bool disposed = false;
        public void Dispose()
        {
            disposed = true;

        }
        public Core()
        {
            System.Threading.Thread t = new System.Threading.Thread(helloLoop);
            t.IsBackground = true;
            t.Name = "Hello send loop";
            t.Start();
            doReceive();
        }

        void doReceive()
        {
            while (!disposed)
            {
                try
                {
                    Program.udp.BeginReceive(receiveCallback, null);
                    return;
                }
                catch
                {
                    System.Threading.Thread.Sleep(10);
                }
            }
        }
        void receiveCallback(IAsyncResult ar)
        {
            System.Net.IPEndPoint sender = null;
            byte[] data = null;
            try
            {
                data = Program.udp.EndReceive(ar, ref sender);
            }
            catch
            {
                doReceive();
                return;
            }
            if(data.Length > 32) //Ignore extraneous STUN info
                parse(Program.serializer.deserialize(data), sender);
            doReceive();
        }
        void parse(Commands.Command c, System.Net.IPEndPoint sender)
        {
            if (c is Commands.HelloCommand)
            {
                Commands.HelloCommand h = (Commands.HelloCommand)c;

            }
        }
        void helloLoop()
        {
            while (!disposed)
            {
                Commands.HelloCommand c = new Commands.HelloCommand();
                c.username = Program.settings.getString("Username", "Username");
                c.machineName = Environment.MachineName;
                c.externalIP = Program.bootstrap.publicControlEndPoint.Address.ToString();
                //todo: set peer count
                c.externalControlPort = Program.bootstrap.publicControlEndPoint.Port;
                c.externalDataPort = Program.bootstrap.publicDataEndPoint.Port;
                c.internalControlPort = Program.bootstrap.internalControlPort;
                c.internalDataPort = Program.bootstrap.internalDataPort;

                //too much output!
                /*var n = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
                List<string> ips = new List<string>();
                for (int i = 0; i < n.Length; i++)
                    foreach (var ni in n[i].GetIPProperties().UnicastAddresses)
                        ips.Add(ni.Address.ToString());
                c.internalIPs = ips.ToArray();
                */

                byte[] b = Program.serializer.serialize(c);

                Program.udp.Send(b, b.Length, new System.Net.IPEndPoint(System.Net.IPAddress.Broadcast, NetConstants.controlPort));

                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}
