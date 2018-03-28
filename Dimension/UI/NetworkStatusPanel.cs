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
            eventsBox.Font = App.getFont();
            portsTextBox.Font = App.getFont();
            trafficBox.Font = App.getFont();
            systemLogBox.Font = App.getFont();
        }

        void update()
        {
            string s = "";

            s += "Internal IP Addresses: ";
            foreach (System.Net.IPAddress a in App.theCore.internalIPs)
                s += a.ToString() + " ";
            s += Environment.NewLine;
            s += "External IP Address: " + App.bootstrap.publicDataEndPoint.Address.ToString() + Environment.NewLine;

            s += "Internal TCP Port: " + App.bootstrap.internalDataPort.ToString() + Environment.NewLine;
            s += "External TCP Port: " + App.bootstrap.publicDataEndPoint.Port.ToString() + Environment.NewLine;

            s += "Internal UDP Port: " + App.bootstrap.internalControlPort.ToString() + Environment.NewLine;
            s += "External UDP Port: " + App.bootstrap.publicControlEndPoint.Port.ToString() + Environment.NewLine;

            s += "Internal Kademlia (UDP) Port: " + App.bootstrap.internalDHTPort.ToString() + Environment.NewLine;
            s += "External Kademlia (UDP) Port: " + App.bootstrap.publicDHTPort.ToString() + Environment.NewLine;
            

            portsTextBox.Text = s;

            s = "";
            s += "Successful UDP command receives from other machines: " + App.theCore.udpCommandsNotFromUs.ToString() + Environment.NewLine;
            s += "Successful incoming TCP connections: " + App.theCore.incomingTcpConnections.ToString() + Environment.NewLine;
            s += "Successful incoming UDT connections: " + App.theCore.incomingUdtConnections.ToString() + Environment.NewLine;
            s += "Successful outgoing TCP connections: " + Model.ReliableOutgoingConnection.successfulConnections + Environment.NewLine;
            s += "Successful outgoing UDT connections: " + Model.UdtOutgoingConnection.successfulConnections.ToString() + Environment.NewLine;


            eventsBox.Text = s;

            s = "*** Protocol Traffic Analysis ***" + Environment.NewLine;
            s += "(Excluding bulk data such as file transfers)" + Environment.NewLine;
            s += Environment.NewLine;
            s += "*** Incoming ***" + Environment.NewLine;
            lock (App.theCore.incomingTraffic)
                foreach (string t in App.theCore.incomingTraffic.Keys)
                    s += t + ": " + ByteFormatter.formatBytes(App.theCore.incomingTraffic[t]) + Environment.NewLine;

            s += Environment.NewLine;
            s += "*** Outgoing ***" + Environment.NewLine;
            lock (App.theCore.outgoingTraffic)
                foreach (string t in App.theCore.outgoingTraffic.Keys)
                    s += t + ": " + ByteFormatter.formatBytes(App.theCore.outgoingTraffic[t]) + Environment.NewLine;

            trafficBox.Text = s;


            systemLogBox.Text = Model.SystemLog.theLog;

            updateFont();
        }
    }
}
