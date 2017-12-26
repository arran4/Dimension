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
        RaptorDB.RaptorDB<string> settings;

        public Settings()
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            folder = Path.Combine(folder, "Dimension");

            Program.currentLoadState = "Loading Settings...";
            settings = new RaptorDB.RaptorDB<string>(Path.Combine(folder, "Settings"), false);
        }
        public void setBool(string name, bool val)
        {

            settings.Set(name, val.ToString());
        }
        public void save()
        {
            settings.SaveIndex();
        }

        public bool getBool(string name, bool def)
        {
            string s = def.ToString();
            settings.Get(name, out s);
            if (s == "" || s == null)
                s = def.ToString();
            return bool.Parse(s);
        }
        }
}
