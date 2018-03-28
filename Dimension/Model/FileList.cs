using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    class FileList : IDisposable
    {
        public event UpdateCompleteEvent updateComplete;
        public delegate void UpdateCompleteEvent();
        Dictionary<string, System.IO.FileSystemWatcher> watchers = new Dictionary<string, System.IO.FileSystemWatcher>();
        //TODO: When updating shares, chew through File IDs less prodigiously
        //TODO: Update bottom-up instead of top-down -- so you don't need to do a complete list rebuild every time you change a file
        public void update(bool urgent)
        {
            lock (updateLock)
            {
                SystemLog.addEntry("Updating all shares" + (urgent ? " (urgently)" : ""));
                RootShare[] shares = Program.fileListDatabase.getRootShares();
                foreach (RootShare r in shares)
                    if (r != null)
                    {
                        updateRootShare(r, urgent);
                        if (System.IO.Directory.Exists(r.fullPath))
                        {
                            try
                            {
                                if (!watchers.ContainsKey(r.fullPath))
                                {
                                    watchers[r.fullPath] = new System.IO.FileSystemWatcher(r.fullPath);
                                    watchers[r.fullPath].Changed += partialUpdate;
                                    watchers[r.fullPath].Created += partialUpdate;
                                    watchers[r.fullPath].Deleted += partialUpdate;
                                    watchers[r.fullPath].Renamed += partialUpdate;
                                    watchers[r.fullPath].IncludeSubdirectories = true;
                                    watchers[r.fullPath].EnableRaisingEvents = true;
                                }
                            }
                            catch (NotImplementedException)
                            {
                                //probably on mono, do nothing
                            }

                        }
                    }
            }
            quitComplete = true;
            quitSemaphore.Release();
            SystemLog.addEntry("Share update complete.");
            if (updateComplete != null)
                updateComplete();
        }
        public void Dispose()
        {
            quitting = true;
            if(!quitComplete)
                quitSemaphore.WaitOne();
        }
        bool quitComplete = false;
        System.Threading.Semaphore quitSemaphore = new System.Threading.Semaphore(0,int.MaxValue);
        bool quitting = false;
        object updateLock = new object();
        Dictionary<string, FSListing> toSave = new Dictionary<string, FSListing>();
        void partialUpdate(object sender, System.IO.FileSystemEventArgs e)
        {
            lock (updateLock)
            {
                quitComplete = false;
                RootShare[] shares = Program.fileListDatabase.getRootShares();
                string path = e.FullPath.Replace('\\', '/');
                SystemLog.addEntry("Partial filesystem update to " + path.Replace('/', System.IO.Path.DirectorySeparatorChar));

                bool isFolder = System.IO.Directory.Exists(path);

                if (!isFolder)
                {
                    path = path.Substring(0, path.LastIndexOf('/')+1);
                    isFolder = System.IO.Directory.Exists(path);
                }
                if (isFolder)
                {
                    System.Threading.Thread t = new System.Threading.Thread(delegate ()
                    {
                        foreach (RootShare r in shares)
                            if (r != null)
                            {
                                if (quitComplete)
                                    return;
                                if (path.StartsWith(r.fullPath + "/"))
                                {
                                    string remaining = path.Replace(System.IO.Path.DirectorySeparatorChar, '/').Substring(r.fullPath.Length + 1);
                                    FSListing f = getFSListing("/"+(r.name + "/" + remaining).Trim('/'), true);

                                    if (f is Folder)
                                    {
                                        deleteFolder((Folder)f, false);
                                        loadFolder((Folder)f, false, path); //TODO: Update size of everything above this folder
                                    }
                                }
                            }
                        doSave();
                        if (updateComplete != null)
                            updateComplete();
                    });
                    t.IsBackground = true;
                    t.Name = "Partial file list update thread";
                    t.Start();
                }
            }
        }
        //TODO: Make this cache in the key-value store instead of just iterating
        public RootShare getRootShare(ulong id)
        {
            return Program.fileListDatabase.getObject<RootShare>(Program.fileListDatabase.fileList, "FSListing " + id.ToString());
        }
        public FSListing getFolder(ulong id)
        {
            return Program.fileListDatabase.getObject<Folder>(Program.fileListDatabase.fileList, "FSListing " + id.ToString());
        }
        public FSListing getFile(ulong id)
        {
            return Program.fileListDatabase.getObject<File>(Program.fileListDatabase.fileList, "FSListing " + id.ToString());
        }
        public FSListing getFSListing(string path, bool folder)
        {
            string[] split = path.Split('/');
            if (split.Length <= 1)
                return null;
            else
            {

                RootShare[] shares = Program.fileListDatabase.getRootShares();
                foreach (RootShare r in shares)
                    if (r != null)
                    {
                        if (r.name == split[1])
                        {
                            if (split.Length == 2)
                                if (folder)
                                    return getFolder(r.id);
                                else
                                    return getFile(r.id);
                            else
                                return getFSListing(r, path);
                        }
                    }

            }
            return null;
        }
        public FSListing getFSListing(Folder parent, string path)
        {
            Folder start = parent;
            tryAgain:
            parent = start;
            string[] split = path.Split('/');

            if (split.Length <2)
                return parent;

            for (int i = 2; i < split.Length; i++)
            {
                bool found = false;
                foreach (ulong id in parent.folderIds)
                {
                    Folder f = Program.fileListDatabase.getObject<Folder>(Program.fileListDatabase.fileList, "FSListing " + id.ToString());

                    if (f == null)
                    {
                        System.Threading.Thread.Sleep(100);
                        goto tryAgain;
                    }

                    if (f.name == split[i])
                    {
                        if (i == split.Length - 1)
                            return f;
                        parent = f;
                        found = true;
                        break;
                    }
                }
                foreach (ulong id in parent.fileIds)
                {
                    File f = Program.fileListDatabase.getObject<File>(Program.fileListDatabase.fileList, "FSListing " + id.ToString());
                    if (f.name == split[i] && i == split.Length - 1)
                    {
                        return f;
                    }
                }
                if (!found)
                    return null;    //couldn't find it
            }
           
            return null;
        }
        System.Diagnostics.Stopwatch sw;
        void wait(bool urgent)
        {
            if (!urgent && sw.ElapsedMilliseconds > 25)
            {
                System.Threading.Thread.Sleep(1);
                sw.Reset();
                sw.Start();
            }
            }
        void updateRootShare(RootShare f, bool urgent)
        {
            lock (toSave)
                toSave.Clear();
            if (quitting)
                return;
            f.id = Program.fileListDatabase.allocateId();
            ulong size = 0;
            SystemLog.addEntry("Updating root share " + f.fullPath.Replace('/', System.IO.Path.DirectorySeparatorChar) + "...");
            sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            string path = "";
            path = f.fullPath;
            
            bool invalidated = false;
            System.IO.DirectoryInfo d = new System.IO.DirectoryInfo(path);
            if (d.LastWriteTimeUtc.Ticks != f.lastModified)
                invalidated = true;
            string s = "";
            try
            {
                if (d.GetFiles().Length + d.GetDirectories().Length != f.folderIds.Length + f.fileIds.Length)
                    invalidated = true;
                foreach (System.IO.FileInfo i in d.GetFiles())
                {
                    s += i.Name + "|" + i.Length.ToString() + "|" + i.LastWriteTimeUtc.Ticks.ToString() + Environment.NewLine;
                    wait(urgent);

                }
                foreach (System.IO.DirectoryInfo i in d.GetDirectories())
                {
                    s += i.Name + "|" + i.LastWriteTimeUtc.Ticks.ToString() + Environment.NewLine;
                    wait(urgent);

                }
            }
            catch (System.IO.IOException)
            {
                return;
            }
            string s2 = "";
            foreach (ulong id in f.fileIds)
            {
                File i = Program.fileListDatabase.getObject<File>(Program.fileListDatabase.fileList, "FSListing " + id.ToString());

                if (i != null)
                {
                    size += i.size;
                    s2 += i.name + "|" + i.size + "|" + i.lastModified.ToString() + Environment.NewLine;
                }
                wait(urgent);
            }
            foreach (ulong id in f.folderIds)
            {
                Folder i = Program.fileListDatabase.getObject<Folder>(Program.fileListDatabase.fileList, "FSListing " + id.ToString());

                if (i != null)
                {
                    size += i.size;
                    s2 += i.name + "|" + i.lastModified.ToString() + Environment.NewLine;
                }
                wait(urgent);
            }
            if (s != s2)
                invalidated = true;
            
            if (invalidated)
            {
                deleteFolder(f, urgent);
                size = loadFolder(f, urgent, path);
                f.size = size;
                lock (toSave)
                    toSave["FSListing " + f.id] = f;
                if (!quitting)
                {
                    Program.fileListDatabase.setObject(Program.settings.settings, "Root Share " + f.index.ToString(), f);
                    doSave();
                }
            }
            sw.Stop();
            sw.Reset();
        }
        public void doSave()
        {
            lock (toSave)
            {
                foreach (string s3 in toSave.Keys)
                    Program.fileListDatabase.setObject(Program.fileListDatabase.fileList, s3, toSave[s3]);

                foreach (string s3 in toSave.Keys)
                {
                    var obj = toSave[s3];
                    string s = obj.name;

                    addId(s, obj.id);

                    foreach (string g in s.Split(new char[] { ' ', '.', '_', '-', '[', ']', '(', ')' }))
                        if (g.Trim().Length >= 4)
                            addId(g.Trim().ToLower(), obj.id);
                }
                toSave.Clear();
            }

        }
        void addId(string s, ulong id)
        {
            ulong[] ids = Program.fileListDatabase.getObject<ulong[]>(Program.fileListDatabase.searchList, s);
            if (ids == null)
                ids = new ulong[0];
            if (ids.Contains(id))
                return;
            Array.Resize(ref ids, ids.Length + 1);
            ids[ids.Length - 1] = id;
            Program.fileListDatabase.setObject<ulong[]>(Program.fileListDatabase.searchList, s, ids);
        }
        public void startUpdate(bool urgent)
        {
            System.Threading.Thread t = new System.Threading.Thread(delegate() { update(urgent); });
            t.IsBackground = true;
            t.Name = "File list update thread";
            t.Start();
        }
        ulong loadFolder(Folder f, bool urgent, string realLocation)
        {
            if (!System.IO.Directory.Exists(realLocation))
                return 0;   //no such folder
            ulong total = 0;
            wait(urgent);
            System.IO.DirectoryInfo d = new System.IO.DirectoryInfo(realLocation);
            lock (toSave)
                toSave["FSListing " + f.id.ToString()]= f;   //save it once here in case the user exits halfway through

            try
            {
                d.GetDirectories();
            }
            catch
            {
                return 0;
            }
            Folder[] folderChildren = new Folder[d.GetDirectories().Length];
            File[] fileChildren = new File[d.GetFiles().Length];
            int fi = 0;
            try
            {
                foreach (System.IO.FileInfo z in d.GetFiles())
                {
                    if (quitting)
                    {
                        return 0;
                    }
                    wait(urgent);
                    File output = new File();
                    output.id = Program.fileListDatabase.allocateId();
                    output.name = z.Name;
                    output.parentId = f.id;
                    output.size = (ulong)z.Length;
                    output.lastModified = z.LastWriteTimeUtc.Ticks;
                    output.isFolder = false;
                    total += output.size;
                    fileChildren[fi] = output;
                    fi++;
                    lock (toSave)
                        toSave["FSListing " + output.id.ToString()] = output;
                }

                fi = 0;
                foreach (System.IO.DirectoryInfo z in d.GetDirectories())
                {
                    if (quitting)
                    {
                        return 0;
                    }
                    wait(urgent);
                    Folder output = new Folder();
                    output.id = Program.fileListDatabase.allocateId();
                    output.name = z.Name;
                    output.parentId = f.id;
                    output.size = loadFolder(output, urgent, realLocation + "/" + z.Name);
                    output.lastModified = z.LastWriteTimeUtc.Ticks;
                    output.isFolder = true;
                    total += output.size;
                    folderChildren[fi] = output;
                    fi++;
                    lock (toSave)
                        toSave["FSListing " + output.id.ToString()] = output;
                }
            }
            catch (System.IO.IOException)
            {
                return 0;
            }

            FSListing x = new FSListing();

            f.fileIds = new ulong[fileChildren.Length];
            for (int i = 0; i < f.fileIds.Length; i++)
                f.fileIds[i] = fileChildren[i].id;
            f.folderIds = new ulong[folderChildren.Length];
            for (int i = 0; i < f.folderIds.Length; i++)
                f.folderIds[i] = folderChildren[i].id;
            f.lastModified = d.LastWriteTimeUtc.Ticks;
            if (quitting)
            {
                deleteFolder(f, true);
                return 0;
            }
            lock (toSave)
                toSave["FSListing " + f.id.ToString()] = f;
            return total;
        }
        void deleteFolder(Folder f, bool urgent)
        {
            if (f == null)
                return;
            wait(urgent);

            foreach (ulong id in f.folderIds)
            {
                wait(urgent);
                if(id != f.id)  
                   deleteFolder(Program.fileListDatabase.getObject<Folder>(Program.fileListDatabase.fileList, "FSListing " + id.ToString()), urgent);
            }

            foreach (ulong id in f.fileIds)
            {
                wait(urgent);
                Program.fileListDatabase.deleteObject(Program.fileListDatabase.fileList, "FSListing " + id.ToString());
            }
            
            Program.fileListDatabase.deleteObject(Program.fileListDatabase.fileList, "FSListing " + f.id.ToString());

            if (f is RootShare)
            {
                var x= Program.fileListDatabase.getObject<Model.RootShare>(Program.settings.settings, "Root Share " + ((RootShare)f).index.ToString());
                if (x == null)
                    return;
                x.totalBytes = 0;
                x.fileIds = new ulong[] { };
                x.folderIds = new ulong[] { };
                Program.fileListDatabase.setObject<Model.RootShare>(Program.settings.settings, "Root Share " + ((RootShare)f).index.ToString(),x);
            }
            }
    }
}
