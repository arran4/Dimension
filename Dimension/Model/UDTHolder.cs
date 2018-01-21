using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    public class UDTHolder
    {
        public UDTHolder()
        {
            udtListener = new Udt.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream);
            udtListener.Bind(System.Net.IPAddress.Any, 0);
            udtListener.Listen(1000);
        }
        public void launchLoop()
        {
            System.Threading.Thread t = new System.Threading.Thread(udtAcceptLoop);
            t.IsBackground = true;
            t.Name = "UDT Accept Loop";
            t.Start();
        }
        public int listenPort
        {
            get
            {
                if (udtListener == null)
                    return 0;
                return udtListener.LocalEndPoint.Port;
            }
            }
        void udtAcceptLoop()
        {
            while (!Program.theCore.disposed)
            {
                Udt.Socket s = udtListener.Accept();
                var p = new UdtIncomingConnection(s);
                Program.theCore.addIncomingConnection(p);
            }


        }
        Udt.Socket udtListener;
        public int udtInternalPort
        {
            get
            {
                if (udtListener == null)
                    return 0;
                else
                    return udtListener.LocalEndPoint.Port;
            }
        }
    }
}
