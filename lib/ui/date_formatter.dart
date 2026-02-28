/*
 * Original C# Source File: Dimension/UI/DateFormatter.cs
 *
﻿using System;
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

            if (d.Year == DateTime.MinValue.Year)
                return "Updating...";

            if (s.TotalSeconds < 0)
                return "";
            if ((int)s.TotalMinutes < 1)
                return "Just Now";
            if ((int)s.TotalMinutes == 1)
                return "1 Minute Ago";
            if (s.TotalMinutes < 60)
                return ((int)s.TotalMinutes).ToString() + " Minutes Ago";
            if ((int)s.TotalHours== 1)
                return "1 Hour Ago";
            if (s.TotalHours < 24)
                return ((int)s.TotalDays).ToString() + " Hours Ago";
            if ((int)s.TotalDays == 1)
                return "1 Day Ago";
            if (s.TotalDays< 7)
                return ((int)s.TotalDays).ToString() + " Days Ago";
            if ((int)(s.TotalDays/7) == 1)
                return "1 Week Ago";
            if (s.TotalDays < 4 * 7)
                return ((int)(s.TotalDays / 7)).ToString() + " Weeks Ago";
            if ((int)(s.TotalDays/7) == 4)
                return "1 Month Ago";
            if (s.TotalDays < 365)
                return ((int)(s.TotalDays / (4 * 7))).ToString() + " Months Ago";
            if ((int)(s.TotalDays / 365 ) == 1)
                return "1 Year Ago";
            return ((int)(s.TotalDays / 365)).ToString() + " Years Ago";
        }
    }
}

*/
