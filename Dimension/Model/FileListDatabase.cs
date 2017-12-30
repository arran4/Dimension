﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Dimension.Model
{
    public class FileListDatabase
    {
        public RaptorDB.RaptorDB<string> fileList;
        public RaptorDB.RaptorDB<string> remoteFileLists;
        public RaptorDB.RaptorDB<string> quickHashes;
        public RaptorDB.RaptorDB<string> fullHashes;
        public RaptorDB.RaptorDB<string> downloadQueue;

        public void saveAll()
        {
            save(fileList);
            save(remoteFileLists);
            save(quickHashes);
            save(fullHashes);
            save(downloadQueue);
        }
        public void save(RaptorDB.RaptorDB<string> db)
        {
            db.Dispose();
        }
        public FileListDatabase()
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            folder = Path.Combine(folder, "Dimension");

            Program.currentLoadState = "Loading File List...";
            fileList = new RaptorDB.RaptorDB<string>(Path.Combine(folder, "FileList"), false);
            Program.currentLoadState = "Loading Hashes...";
            quickHashes = new RaptorDB.RaptorDB<string>(Path.Combine(folder, "QuickHashes"), false);
            fullHashes = new RaptorDB.RaptorDB<string>(Path.Combine(folder, "FullHashes"), false);
            downloadQueue = new RaptorDB.RaptorDB<string>(Path.Combine(folder, "DownloadQueue"), false);
            remoteFileLists = new RaptorDB.RaptorDB<string>(Path.Combine(folder, "RemoteFileLists"), false);
        }
        public void setString(RaptorDB.RaptorDB<string> db, string name, string val)
        {
            db.Set("s" + name, val);
        }
        public string getString(RaptorDB.RaptorDB<string> db, string name, string def)
        {
            string s = def;
            db.Get("s" + name, out s);
            if (s == "" || s == null)
                s = def.ToString();
            return s;
        }
        public ulong allocateId()
        {
            ulong u = getULong(fileList, "Current FSListing ID", 0);
            u++;
            setULong(fileList, "Current FSListing ID", u);
            return u;
        }
        public RootShare[] getRootShares()
        {
            int numShares = getInt(fileList, "Root Share Count", 0);
            RootShare[] output = new RootShare[numShares];
            for (int i = 0; i < numShares; i++)
                output[i] = getObject<Model.RootShare>(Program.settings.settings, "Root Share " + i.ToString());
            return output;
        }
        public void deleteObject(RaptorDB.RaptorDB<string> db, string name)
        {
            db.RemoveKey("o" + name);
        }
        public void setObject<T>(RaptorDB.RaptorDB<string> db, string name, T val)
        {
            db.Set("o" + name, Newtonsoft.Json.JsonConvert.SerializeObject(val));
        }
        public T getObject<T>(RaptorDB.RaptorDB<string> db, string name)
        {
            string s = null;
            db.Get("o" + name, out s);
            if (s == null)
                return default(T);
            else
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(s);
        }
        public void setInt(RaptorDB.RaptorDB<string> db, string name, int val)
        {

            db.Set("i" + name, val.ToString());
        }
        public void setULong(RaptorDB.RaptorDB<string> db, string name, ulong val)
        {

            db.Set("i" + name, val.ToString());
        }
        public int getInt(RaptorDB.RaptorDB<string> db, string name, int def)
        {
            string s = def.ToString();
            try
            {
                db.Get("i" + name, out s);
            }
            catch
            {
                return def;
            }
                if (s == "" || s == null)
                s = def.ToString();
            return int.Parse(s);
        }
        public ulong getULong(RaptorDB.RaptorDB<string> db, string name, ulong def)
        {
            string s = def.ToString();
            db.Get("i" + name, out s);
            if (s == "" || s == null)
                s = def.ToString();
            return ulong.Parse(s);
        }
    }
}
