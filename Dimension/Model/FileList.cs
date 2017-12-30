using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    class FileList : IDisposable
    {
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
            }
            quitComplete = true;
            quitSemaphore.Release();
            SystemLog.addEntry("Share update complete.");
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
        Dictionary<string, FSListing> toSave;
        void partialUpdate(object sender, System.IO.FileSystemEventArgs e)
        {
            lock (updateLock)
            {
                RootShare[] shares = Program.fileListDatabase.getRootShares();
                string path = e.FullPath.Replace('\\', '/');
                SystemLog.addEntry("Partial filesystem update to " + path.Replace('/', System.IO.Path.DirectorySeparatorChar));

                bool isFolder = System.IO.Directory.Exists(path);
                if (isFolder)
                {
                    foreach (RootShare r in shares)
                        if (r != null)
                        {
                            if (quitComplete)
                                return;
                            if ((path + "/").StartsWith(r.fullPath + "/"))
                            {
                                string remaining = path.Substring(r.fullPath.Length + 1);
                                FSListing f = getFSListing(r, remaining);

                                if (f is Folder)
                                {
                                    deleteFolder((Folder)f, false);
                                    loadFolder((Folder)f, false, path); //TODO: Update size of everything above this folder
                                }
                            }
                        }
                }
            }
        }
        //TODO: Make this cache in the key-value store instead of just iterating
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
            return null;
        }
        public FSListing getFSListing(Folder parent, string path)
        {
            string[] split = path.Split('/');

            for (int i = 0; i < split.Length; i++)
            {
                bool found = false;
                foreach (ulong id in parent.folderIds)
                {
                    Folder f = Program.fileListDatabase.getObject<Folder>(Program.fileListDatabase.fileList, "FSListing " + id.ToString());
                    
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
            toSave = new Dictionary<string, FSListing>();
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
            if (d.GetFiles().Length + d.GetDirectories().Length != f.folderIds.Length + f.fileIds.Length)
                invalidated = true;
            
            string s = "";
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
            string s2 = "";
            foreach (ulong id in f.fileIds)
            {
                File i = Program.fileListDatabase.getObject<File>(Program.fileListDatabase.fileList, "FSListing " + id.ToString());

                size += i.size;
                s2 += i.name + "|" + i.size + "|" + i.lastModified.ToString() + Environment.NewLine;
                wait(urgent);
            }
            foreach (ulong id in f.folderIds)
            {
                Folder i = Program.fileListDatabase.getObject<Folder>(Program.fileListDatabase.fileList, "FSListing " + id.ToString());

                size += i.size;
                s2 += i.name + "|" + i.lastModified.ToString() + Environment.NewLine;
                wait(urgent);
            }
            if (s != s2)
                invalidated = true;
            
            if (invalidated)
            {
                deleteFolder(f, urgent);
                size = loadFolder(f, urgent, path);
                f.size = size;
                toSave["FSListing " + f.id] = f;
                Program.fileListDatabase.setObject(Program.settings.settings, "Root Share " + f.index.ToString(), f);
                foreach(string s3 in toSave.Keys)
                    Program.fileListDatabase.setObject(Program.fileListDatabase.fileList, s3, toSave[s3]);
                toSave = null;
            }
            sw.Stop();
            sw.Reset();
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
            ulong total = 0;
            wait(urgent);
            System.IO.DirectoryInfo d = new System.IO.DirectoryInfo(realLocation);
            toSave["FSListing " + f.id.ToString()]= f;   //save it once here in case the user exits halfway through

            Folder[] folderChildren = new Folder[d.GetDirectories().Length];
            File[] fileChildren = new File[d.GetFiles().Length];
            int fi = 0;
            foreach (System.IO.FileInfo z in d.GetFiles())
            {
                if (quitting)
                {
                    deleteFolder(f, true);
                    return 0;
                }
                wait(urgent);
                File output = new File();
                output.id = Program.fileListDatabase.allocateId();
                output.name = z.Name;
                output.parentId = f.id;
                output.size = (ulong)z.Length;
                output.lastModified = z.LastWriteTimeUtc.Ticks;
                total += output.size;
                fileChildren[fi] = output;
                fi++;
                toSave["FSListing " + output.id.ToString()] = output;
            }
            
            fi = 0;
            foreach (System.IO.DirectoryInfo z in d.GetDirectories())
            {
                if (quitting)
                {
                    deleteFolder(f, true);
                    return 0;
                }
                wait(urgent);
                Folder output = new Folder();
                output.id = Program.fileListDatabase.allocateId();
                output.name = z.Name;
                output.parentId = f.id;
                output.size = loadFolder(output, urgent, realLocation + "/" + z.Name);
                output.lastModified = z.LastWriteTimeUtc.Ticks;
                total += output.size;
                folderChildren[fi] = output;
                fi++;
                toSave["FSListing " + output.id.ToString()] = output;
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
                x.totalBytes = 0;
                x.fileIds = new ulong[] { };
                x.folderIds = new ulong[] { };
                Program.fileListDatabase.setObject<Model.RootShare>(Program.settings.settings, "Root Share " + ((RootShare)f).index.ToString(),x);
            }
            }
    }
}
