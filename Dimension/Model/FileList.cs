﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    class FileList
    {
        //TODO: When updating shares, chew through File IDs less prodigiously
        //TODO: Update bottom-up instead of top-down -- so you don't need to do a complete list rebuild every time you change a file
        public void update(bool urgent)
        {
            SystemLog.addEntry("Updating all shares" + ( urgent ? " (urgently)" : ""));
            RootShare[] shares = Program.fileListDatabase.getRootShares();
            foreach (RootShare r in shares)
                if(r!=null)
                    updateRootShare(r, urgent);

            SystemLog.addEntry("Share update complete.");
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
        void updateRootShare(RootShare f, bool urgent)
        {
            ulong size = 0;
            SystemLog.addEntry("Updating root share " + f.fullPath + "...");
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
            }
            f.size = size;
            Program.fileListDatabase.setObject(Program.fileListDatabase.fileList, "Root Share "+f.index.ToString(), f);
            sw.Stop();
            sw.Reset();
            Program.fileListDatabase.saveAll();
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

            Folder[] folderChildren = new Folder[d.GetDirectories().Length];
            File[] fileChildren = new File[d.GetFiles().Length];
            int fi = 0;
            foreach (System.IO.FileInfo z in d.GetFiles())
            {
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
                Program.fileListDatabase.setObject(Program.fileListDatabase.fileList, "FSListing " + output.id.ToString(), output);
            }
            
            fi = 0;
            foreach (System.IO.DirectoryInfo z in d.GetDirectories())
            {
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
                Program.fileListDatabase.setObject(Program.fileListDatabase.fileList, "FSListing " + output.id.ToString(), output);
            }
            FSListing x = new FSListing();

            f.fileIds = new ulong[fileChildren.Length];
            for (int i = 0; i < f.fileIds.Length; i++)
                f.fileIds[i] = fileChildren[i].id;
            f.folderIds = new ulong[folderChildren.Length];
            for (int i = 0; i < f.folderIds.Length; i++)
                f.folderIds[i] = folderChildren[i].id;
            f.lastModified = d.LastWriteTimeUtc.Ticks;
            Program.fileListDatabase.setObject(Program.fileListDatabase.fileList, "FSListing " + f.id.ToString(), f);
            return total;
        }
        void deleteFolder(Folder f, bool urgent)
        {
            wait(urgent);

            foreach (ulong id in f.folderIds)
            {
                wait(urgent);
                deleteFolder(Program.fileListDatabase.getObject<Folder>(Program.fileListDatabase.fileList, "FSListing " + id.ToString()), urgent);
            }

            foreach (ulong id in f.fileIds)
            {
                wait(urgent);
                Program.fileListDatabase.deleteObject(Program.fileListDatabase.fileList, id.ToString());
            }
            
            Program.fileListDatabase.deleteObject(Program.fileListDatabase.fileList, f.id.ToString());
        }
    }
}
