using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Dimension.Model
{
    public class Kademlia
    {

        public void Dispose()
        {
            dht.Stop();
            dht.Dispose();
        }
        OctoTorrent.Dht.DhtEngine dht;
        System.Threading.Semaphore dhtWait = new System.Threading.Semaphore(0, 1);
        string peerCachePath = System.IO.Path.Combine(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dimension"),"DHT Peer Cache.bin");
        public void initialize()
        {
            //c.Close();
            OctoTorrent.Dht.Listeners.DhtListener dhtl = new OctoTorrent.Dht.Listeners.DhtListener(new IPEndPoint(IPAddress.Any, Program.bootstrap.internalDHTPort));
            dht = new OctoTorrent.Dht.DhtEngine(dhtl);
            dht.PeersFound += peersFound;
            dht.StateChanged += stateChanged;
            dhtl.Start();
            if (System.IO.File.Exists(peerCachePath))
                dht.Start(System.IO.File.ReadAllBytes(peerCachePath));
            else
                dht.Start();

            dht.Start();

            dhtWait.WaitOne();

            
            System.IO.File.WriteAllBytes(peerCachePath, dht.SaveNodes());

        }

        public bool ready = false;
        void stateChanged(object sender, EventArgs a)
        {
            if (dht.State == OctoTorrent.DhtState.Ready && !ready)
            {
                dhtWait.Release();
                ready = true;
            }
        }
        object lookupLock = new object();
        Dictionary<string, IPEndPoint[]> results = new Dictionary<string, IPEndPoint[]>();

        byte[] doHash(string s)
        {
            System.Security.Cryptography.SHA512Managed sha = new System.Security.Cryptography.SHA512Managed();
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(s));
            byte[] output = new byte[20];
            Array.Copy(hash, output, output.Length);
            return output;
        }
        public void announce(string key)
        {
            lock (lookupLock)
            {
                byte[] hash = doHash(key);
                dht.Announce(new OctoTorrent.InfoHash(hash), Program.bootstrap.publicControlEndPoint.Port);
            }

        }
        public IPEndPoint[] doLookup(string key)
        {
            lock (lookupLock)
            {
                byte[] hash = doHash(key);

                dht.GetPeers(new OctoTorrent.InfoHash(hash));
                
                dhtWait.WaitOne(30000);

                if (!results.ContainsKey(hash.ToString()))
                    return new IPEndPoint[] { };
                IPEndPoint[] output = results[hash.ToString()];

                return output;
            }
        }
        void peersFound(object sender, OctoTorrent.PeersFoundEventArgs results)
        {
            IPEndPoint[] output = new IPEndPoint[results.Peers.Count];
            //for (int i = 0; i < output.Length; i++)
            //    output[i] = results.Peers[i].ConnectionUri;
            this.results[results.InfoHash.ToArray().ToString()] = output;
            try
            {
                dhtWait.Release();
            }
            catch
            {
                //do nothing
            }

        }
    }
}
