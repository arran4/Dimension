﻿using System;
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
        Dictionary<string, string> cache = new Dictionary<string, string>();

        public Settings()
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            folder = Path.Combine(folder, "Dimension");

            SystemLog.addEntry("Loading Settings...");
            settings = new RaptorDB.RaptorDB<string>(Path.Combine(folder, "Settings"), false);
        }

        public string[] getStringArray(string name)
        {
            if (cache.ContainsKey("qs" + name))
                return Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(cache["qs" + name]);
            string z = "[]";
            settings.Get("qs" + name, out z);
            cache["qs" + name] = z;
            if (z == "" || z == null)
                z = "[]";
            return Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(z);
        }
        public void addStringToArrayNoDup(string name, string value)
        {
            string[] q = getStringArray(name);
            if (q.Contains(value))
                return;
            Array.Resize(ref q, q.Length + 1);
            q[q.Length - 1] = value;
            setStringArray(name, q);
        }
        public void removeStringToArrayNoDup(string name, string value)
        {
            string[] q = getStringArray("qs" + name);
            if (!q.Contains(value))
                return;
            List<string> output = new List<string>();
            output.AddRange(q);
            setStringArray(name, output.ToArray());
        }
        public void setStringArray(string name, string[] value)
        {
            string data = Newtonsoft.Json.JsonConvert.SerializeObject(value);
            cache["qs" + name] = data;
            settings.Set("qs" + name, data);
        }
        public ulong getULong(string name, ulong def)
        {
            if (cache.ContainsKey("i" + name))
                return ulong.Parse(cache["i" + name]);
            string s = def.ToString();
            settings.Get("i" + name, out s);
            if (s == "" || s == null)
                s = def.ToString();
            cache["i" + name] = s;
            return ulong.Parse(s);
        }
        public void setBool(string name, bool val)
        {
            cache["b" + name] = val.ToString();
            settings.Set("b" + name, val.ToString());
        }
        public bool getBool(string name, bool def)
        {
            if (cache.ContainsKey("b" + name))
                return bool.Parse(cache["b" + name]);
            string s = def.ToString();
            settings.Get("b"+name, out s);
            if (s == "" || s == null)
                s = def.ToString();
            cache["b" + name] = s;
            return bool.Parse(s);
        }
        public void setString(string name, string val)
        {
            cache["s" + name] = val;
            settings.Set("s" + name, val);
        }
        public string getString(string name, string def)
        {
            if (cache.ContainsKey("s" + name))
                return cache["s" + name];
            if (def == "")
                def = " ";
            string s = def;
            try
            {
                settings.Get("s" + name, out s);
            }
            catch
            {
                return def;
            }
            if (s == " " || s=="" || s == null)
                s = def;
            cache["s" + name] = s;
            return s;
        }
        public void setULong(string name, ulong val)
        {

            cache["i" + name] = val.ToString();
            settings.Set("i" + name, val.ToString());
        }
        public void setInt(string name, int val)
        {
            cache["i" + name] = val.ToString();

            settings.Set("i" + name, val.ToString());
        }
        public int getInt(string name, int def)
        {
            if (cache.ContainsKey("i" + name))
                return int.Parse(cache["i" + name]);
            string s = def.ToString();
            settings.Get("i" + name, out s);
            if (s == "" || s == null)
                s = def.ToString();
            cache["i" + name] = s;
            return int.Parse(s);
        }
        public void save()
        {
            settings.SaveIndex();
        }

        }
}
