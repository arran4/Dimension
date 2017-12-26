using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Dimension.Model
{
    public class Settings
    {
        public RaptorDB.RaptorDB<string> settings;

        public Settings()
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            folder = Path.Combine(folder, "Dimension");

            Program.currentLoadState = "Loading Settings...";
            settings = new RaptorDB.RaptorDB<string>(Path.Combine(folder, "Settings"), false);
        }
    }
}
