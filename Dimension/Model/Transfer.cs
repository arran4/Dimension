using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    public class Transfer
    {
        public string originalPath;
        public string path;
        public Peer thePeer;
        public Connection con;
        public bool download;
        public string protocol;
        public string filename;
        public ulong size;
        public ulong startingByte;
        public ulong completed;
        public string username = "";
        public ulong rate;
        public ulong userId;
        public DateTime timeCreated = DateTime.Now;

        public static List<Transfer> transfers = new List<Transfer>();
    }
}
