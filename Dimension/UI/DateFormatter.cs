using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimension.UI
{
    public static class DateFormatter
    {
        public static string formatDate(DateTime d)
        {
            TimeSpan s = DateTime.Now.Subtract(d);

            if (s.TotalSeconds < 0)
                return "";
            if (s.TotalMinutes < 60)
                return ((int)s.TotalDays).ToString() + " Minutes Ago";
            if (s.TotalHours < 24)
                return ((int)s.TotalDays).ToString() + " Hours Ago";
            if (s.TotalDays< 7)
                return ((int)s.TotalDays).ToString() + " Days Ago";
            if (s.TotalDays < 4 * 7)
                return ((int)(s.TotalDays / 7)).ToString() + " Weeks Ago";
            if (s.TotalDays < 365)
                return ((int)(s.TotalDays / (4 * 7))).ToString() + " Months Ago";
            return ((int)(s.TotalDays / 365)).ToString() + " Years Ago";
        }
    }
}
