using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    public class Transfer
    {
        public bool download;
        public string protocol;
        public string filename;
        public ulong size;
        public ulong completed;

        public static List<Transfer> transfers = new List<Transfer>();
    }
}
