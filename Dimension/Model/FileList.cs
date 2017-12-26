using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Dimension.Model
{
    public class FileList
    {
        RaptorDB.RaptorDB<string> fileList;
        RaptorDB.RaptorDB<ulong> quickHashes;
        RaptorDB.RaptorDB<ulong> fullHashes;

        public FileList()
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            folder = Path.Combine(folder, "Dimension");

            Program.currentLoadState = "Loading File List...";
            fileList = new RaptorDB.RaptorDB<string>(Path.Combine(folder, "FileList"), false);
            Program.currentLoadState = "Loading Hashes...";
            quickHashes = new RaptorDB.RaptorDB<ulong>(Path.Combine(folder, "QuickHashes"), false);
            fullHashes = new RaptorDB.RaptorDB<ulong>(Path.Combine(folder, "FullHashes"), false);
        }
    }
}
