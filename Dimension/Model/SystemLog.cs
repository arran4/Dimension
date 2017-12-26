using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    public class SystemLog
    {
        public static string theLog="";
        public static void addEntry(string s)
        {
            theLog += s + Environment.NewLine;
        }
        }
}
