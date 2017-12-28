using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    class FileList
    {
        public void update(bool urgent)
        {
            RootShare[] shares = Program.fileListDatabase.getRootShares();
            foreach (RootShare r in shares)
                updateFolder(r, urgent);
        }
        System.Diagnostics.Stopwatch sw;
        void wait(bool urgent)
        {
            if (!urgent && sw.ElapsedMilliseconds > 50)
            {
                System.Threading.Thread.Sleep(3);
                sw.Reset();
            }
            }
        void updateFolder(Folder f, bool urgent)
        {
            sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            string path = "";
            if (f is RootShare)
            {
                path = ((RootShare)f).fullPath;
            }
            bool invalidated = false;
            System.IO.DirectoryInfo d = new System.IO.DirectoryInfo(path);
            if (d.LastWriteTimeUtc != f.lastModified)
                invalidated = true;
            if (d.GetFiles().Length + d.GetDirectories().Length != f.folderChildren.Length + f.fileChildren.Length)
                invalidated = true;
            if (!invalidated)
            {
                string s = "";
                foreach (System.IO.FileInfo i in d.GetFiles())
                {
                    s += i.Name + "|" + i.Length.ToString() + "|" + i.LastWriteTimeUtc.ToString();
                    wait(urgent);
                }
                foreach (System.IO.DirectoryInfo i in d.GetDirectories())
                {
                    s += i.Name + "|" + i.LastWriteTimeUtc.ToString();
                    wait(urgent);
                }
                string s2 = "";
                foreach (File i in f.fileChildren)
                {
                    s2 += i.name + "|" + i.size + "|" + i.lastModified.ToString();
                    wait(urgent);
                }
                foreach (Folder i in f.folderChildren)
                {
                    s2 += i.name + "|" + i.lastModified.ToString();
                    wait(urgent);
                }
                if (s != s2)
                    invalidated = true;
            }
            if (invalidated)
            {
                deleteFolder(f, urgent);
                loadFolder(f, urgent);
            }
            sw.Stop();
            sw.Reset();
        }
        void loadFolder(Folder f, bool urgent)
        {
            wait(urgent);


        }
        void deleteFolder(Folder f, bool urgent)
        {
            wait(urgent);

            foreach (Folder z in f.folderChildren)
            {
                wait(urgent);
                deleteFolder(z, urgent);
            }

            foreach (File z in f.fileChildren)
            {
                wait(urgent);
                Program.fileListDatabase.deleteObject(Program.fileListDatabase.fileList, z.id.ToString());
            }

            Program.fileListDatabase.deleteObject(Program.fileListDatabase.fileList, f.id.ToString());
        }
    }
}
