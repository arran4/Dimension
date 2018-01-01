using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    public class SystemLog
    {
        public static string lastLine = "";
        public static string theLog="";
        static System.IO.FileStream log;
        public static System.IO.StreamWriter writer;
        public static void addEntry(string s)
        {
            if (log == null)
            {
                string w = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Dimension";
                if (!System.IO.Directory.Exists(w))
                    System.IO.Directory.CreateDirectory(w);
                log = System.IO.File.Open(w + "/Log.txt", System.IO.FileMode.Create);
                writer = new System.IO.StreamWriter(log);
            }
            lastLine = s;
            theLog += s + Environment.NewLine;
            writer.WriteLine(s);
            writer.Flush();
        }
        }
}
