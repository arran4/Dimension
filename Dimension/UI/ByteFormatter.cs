using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.UI
{
    public static class ByteFormatter
    {
        static string[] suffixes = new string[] { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"};
        public static string formatBytes(ulong b)
        {
            double num = b;
            int suffixNum = 0;
            while (num > 1024)
            {
                num /= 1024.0;
                suffixNum++;
            }
            return (((int)(num * 100)) / 100.0).ToString() + suffixes[suffixNum];
            }
        }
}
