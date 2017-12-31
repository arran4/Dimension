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

        List<DateTime> dataAdded = new List<DateTime>();
        List<ulong> dataAmount = new List<ulong>();
        public void addData(ulong amount)
        {
            lock (dataAdded)
            {
                dataAdded.Add(DateTime.Now);
                dataAmount.Add(amount);
                completed += amount;
            }
        }
        public ulong rate()
        {
            lock (dataAdded)
            {
                List<int> toRemove = new List<int>();
                for (int i = 0; i < dataAdded.Count; i++)
                    if (DateTime.Now.Subtract(dataAdded[i]).TotalMilliseconds > 2000)
                        toRemove.Add(i);
                for (int i = toRemove.Count - 1; i >= 0; i--)
                {
                    dataAdded.RemoveAt(i);
                    dataAmount.RemoveAt(i);
                }
                ulong output = 0;
                foreach (ulong i in dataAmount)
                    output += i;
                return output / 2;
            }
        }

        public static List<Transfer> transfers = new List<Transfer>();
    }
}
