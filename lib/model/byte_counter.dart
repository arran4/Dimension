/*
 * Original C# Source File: DimensionLib/Model/ByteCounter.cs
 *
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    public class ByteCounter
    {
        public ulong totalBytes = 0;
        public ulong frontBuffer = 0;
        ulong backBuffer = 0;
        public void addBytes(int u)
        {
            totalBytes += (ulong)u;
            backBuffer += (ulong)u;
        }
        public void addBytes(ulong u)
        {
            totalBytes += u;
            backBuffer += u;
        }
        static bool updateRunning = false;
        static List<ByteCounter> allCounters = new List<ByteCounter>();
        public ByteCounter()
        {
            if (!updateRunning)
            {
                updateRunning = true;
                System.Threading.Thread t = new System.Threading.Thread(byteUpdateLoop);
                t.Name = "Byte counter update loop";
                t.IsBackground = true;
                t.Start();
            }
            lock (allCounters)
                allCounters.Add(this);
        }
        void update()
        {

            frontBuffer = backBuffer;
            backBuffer = 0;
        }
        void byteUpdateLoop()
        {
            while (true)
            {
                lock (allCounters)
                    foreach (ByteCounter c in allCounters)
                        c.update();
                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}

*/
