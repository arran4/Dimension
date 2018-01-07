using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.Model
{
    class GlobalSpeedLimiter : IDisposable
    {
        bool disposed = false;
        public void Dispose()
        {
            disposed = true;
        }
        public GlobalSpeedLimiter()
        {
            System.Threading.Thread t = new System.Threading.Thread(updateLoop);
            t.Name = "Global speed limit manager";
            t.IsBackground = true;
            t.Start();
        }

        ulong totalUpload;
        ulong totalDownload;

        ulong currentDownloadLimit;
        ulong currentUploadLimit;
        void updateLoop()
        {
            while (!disposed)
            {
                totalDownload = 0;
                totalUpload = 0;

                currentDownloadLimit = Program.settings.getULong("Global Download Rate Limit", 0);
                currentUploadLimit = Program.settings.getULong("Global Upload Rate Limit", 0);


                System.Threading.Thread.Sleep(100);
            }
        }
        public void limitUpload(ulong amount)
        {
            if (currentUploadLimit > 0)
            {

            }

            }
        public void limitDownload(ulong amount)
        {
            if (currentDownloadLimit > 0)
            {

            }

        }
    }
}
