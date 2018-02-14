// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Shared.Extensions
{
    public static class Times
    {
        public static string PubDatetoString(this DateTime d, string todayStr, string dayPattern, string datePattern, string yearDatePattern, string nextString, string lastString)
        {
            var a = DateTime.Today;
            var k = (a - d);

            if (d.Date == a)
            {
                return d.ToString(todayStr);
            }

            if (d.Year != a.Year)
            {
                return d.ToString(yearDatePattern);
            }
            else
            {
                // use Date
                if (Math.Abs(k.TotalDays) > 7)
                {
                    return d.ToString(datePattern);
                }
                // use day of week
                else
                {
                    if (d > a)
                    {
                        // this week
                        if (d.DayOfWeek > a.DayOfWeek)
                        {
                            return d.ToString(dayPattern);
                        }
                        // next week
                        else
                        {
                            return $"{nextString}{d.ToString(dayPattern)}";
                        }
                    }
                    else
                    {
                        // last week
                        if (d.DayOfWeek >= a.DayOfWeek)
                        {
                            return $"{lastString}{d.ToString(dayPattern)}";
                        }
                        // this week
                        else
                        {
                            return d.ToString(dayPattern);
                        }
                    }
                }
            }
        }
    }
}
