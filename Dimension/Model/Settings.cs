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

            settings.Set("b" + name, val.ToString());
        }
        public bool getBool(string name, bool def)
        {
            string s = def.ToString();
            settings.Get("b"+name, out s);
            if (s == "" || s == null)
                s = def.ToString();
            return bool.Parse(s);
        }
        public void setString(string name, string val)
        {

            settings.Set("s" + name, val);
        }
        public string getString(string name, string def)
        {
            string s = def;
            settings.Get("s" + name, out s);
            if (s == "" || s == null)
                s = def.ToString();
            return s;
        }
        public void setInt(string name, int val)
        {

            settings.Set("i" + name, val.ToString());
        }
        public int getInt(string name, int def)
        {
            string s = def.ToString();
            settings.Get("i" + name, out s);
            if (s == "" || s == null)
                s = def.ToString();
            return int.Parse(s);
        }
        public void save()
        {
            settings.SaveIndex();
        }

        }
}
