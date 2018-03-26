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
            OctoTorrent.Client.ClientEngine client = new OctoTorrent.Client.ClientEngine(new OctoTorrent.Client.EngineSettings(), Program.theCore.id.ToString());
             OctoTorrent.Dht.Listeners.DhtListener dhtl = new OctoTorrent.Dht.Listeners.DhtListener(new IPEndPoint(IPAddress.Any, Program.bootstrap.internalDHTPort));
             dht = new OctoTorrent.Dht.DhtEngine(dhtl);

             dht.PeersFound += peersFound;
             dht.StateChanged += stateChanged;
             dhtl.Start();
             
            /*if (System.IO.File.Exists(peerCachePath))
                dht.Start(System.IO.File.ReadAllBytes(peerCachePath));
            else*/
            dht.Start();
            
            dhtWait.WaitOne();

            
            System.IO.File.WriteAllBytes(peerCachePath, dht.SaveNodes());
            
        }

        public bool ready = false;
        void stateChanged(object sender, EventArgs a)
        {
            if (dht.State == OctoTorrent.DhtState.Ready && !ready)
            {
                try
                {
                    dhtWait.Release();
                }
                catch
                {
                }
                ready = true;
            }
        }
        object lookupLock = new object();
        Dictionary<string, List<IPEndPoint>> results = new Dictionary<string, List<IPEndPoint>>();

        byte[] doHash(string s)
        {
            System.Security.Cryptography.SHA512Managed sha = new System.Security.Cryptography.SHA512Managed();
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(s.ToLower()));
            byte[] output = new byte[20];
            Array.Copy(hash, output, output.Length);
            return output;
        }
        public void announce(string key)
        {
            lock (lookupLock)
            {
                try
                {
                    byte[] hash = doHash(key);
                    dht.Announce(new OctoTorrent.InfoHash(hash), Program.bootstrap.publicControlEndPoint.Port);
                }
                catch (ObjectDisposedException)
                {
                }
            }
            
        }
        public IPEndPoint[] doLookup(string key)
        {
            return new IPEndPoint[] { };
            lock (lookupLock)
            {
                byte[] hash = doHash(key);

                dht.GetPeers(new OctoTorrent.InfoHash(hash));
                
                dhtWait.WaitOne(30000);

                if (!results.ContainsKey(hash.ToString()))
                    return new IPEndPoint[] { };
                IPEndPoint[] output = results[hash.ToString()].ToArray();

                return output;
            }
        }
        void peersFound(object sender, OctoTorrent.PeersFoundEventArgs results)
        {
            IPEndPoint[] output = new IPEndPoint[results.Peers.Count];
            for (int i = 0; i < output.Length; i++)
                output[i] = new IPEndPoint(IPAddress.Parse(results.Peers[i].ConnectionUri.Host), results.Peers[i].ConnectionUri.Port);
            if (!this.results.ContainsKey(results.InfoHash.ToArray().ToString()))
                this.results[results.InfoHash.ToArray().ToString()] = new List<IPEndPoint>();
            foreach (IPEndPoint z in output)
            {
                bool already = false;
                foreach (IPEndPoint w in this.results[results.InfoHash.ToArray().ToString()])
                    if (w.Address.ToString() == z.Address.ToString() && w.Port == z.Port)
                        already = true;
                if (!already)
                    this.results[results.InfoHash.ToArray().ToString()].AddRange(output);
            }
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
