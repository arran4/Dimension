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


                System.Threading.Thread.Sleep(1000);
            }
        }
        public ulong limitUpload(ulong amount)
        {
            if (currentUploadLimit > 0)
            {
                while (totalUpload > currentUploadLimit)
                    System.Threading.Thread.Sleep(1);
                while (Math.Min(amount, currentUploadLimit - totalUpload) <= 0)
                    System.Threading.Thread.Sleep(1);

                totalUpload += amount;
                return Math.Min(amount, currentUploadLimit - (totalUpload-amount));

            }
            else
                return amount;

        }
        public ulong limitDownload(ulong amount)
        {
            if (currentDownloadLimit > 0)
            {
                while (totalDownload > currentDownloadLimit)
                    System.Threading.Thread.Sleep(1);
                while(Math.Min(amount, currentDownloadLimit - totalDownload) <= 0)
                    System.Threading.Thread.Sleep(1);

                totalDownload += amount;
                return Math.Min(amount, currentDownloadLimit - (totalDownload - amount));
            }
            else
                return amount;

        }
    }
}
