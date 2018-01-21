using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dimension.UI
{
    public partial class NetworkStatusPanel : UserControl
    {
        public NetworkStatusPanel()
        {
            InitializeComponent();
            update();
        }
        private void updateTimer_Tick(object sender, EventArgs e)
        {
            update();
        }
        void updateFont()
        {
            eventsBox.Font = Program.getFont();
            portsTextBox.Font = Program.getFont();
        }

        void update()
        {
            string s = "";

            s += "Internal IP Addresses: ";
            foreach (System.Net.IPAddress a in Program.theCore.internalIPs)
                s += a.ToString() + " ";
            s += Environment.NewLine;
            s += "External IP Address: " + Program.bootstrap.publicDataEndPoint.Address.ToString() + Environment.NewLine;

            s += "Internal TCP Port: " + Program.bootstrap.internalDataPort.ToString() + Environment.NewLine;
            s += "External TCP Port: " + Program.bootstrap.publicDataEndPoint.Port.ToString() + Environment.NewLine;

            s += "Internal UDP Port: " + Program.bootstrap.internalControlPort.ToString() + Environment.NewLine;
            s += "External UDP Port: " + Program.bootstrap.publicControlEndPoint.Port.ToString() + Environment.NewLine;

            s += "Internal Kademlia (UDP) Port: " + Program.bootstrap.internalDHTPort.ToString() + Environment.NewLine;
            s += "External Kademlia (UDP) Port: " + Program.bootstrap.publicDHTPort.ToString() + Environment.NewLine;

            if (Program.theCore.udtHolder != null)
                s += "Internal UDT (UDP) Port: " + Program.theCore.udtHolder.udtInternalPort.ToString() + Environment.NewLine;
            else
                s += "UDT disabled.";

            portsTextBox.Text = s;

            s = "";
            s += "Successful UDP command receives from other machines: " + Program.theCore.udpCommandsNotFromUs.ToString() + Environment.NewLine;
            s += "Successful incoming TCP connections: " + Program.theCore.incomingTcpConnections.ToString() + Environment.NewLine;
            s += "Successful incoming UDT connections: " + Program.theCore.incomingUdtConnections.ToString() + Environment.NewLine;
            s += "Successful outgoing TCP connections: " + Model.ReliableOutgoingConnection.successfulConnections + Environment.NewLine;
            s += "Successful outgoing UDT connections: " + Model.UdtOutgoingConnection.successfulConnections.ToString() + Environment.NewLine;


            eventsBox.Text = s;
            updateFont();
        }
    }
}
