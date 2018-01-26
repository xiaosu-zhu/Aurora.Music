// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Aurora.Shared.Helpers
{
    public static class DateTimeHelper
    {
        private static ReadOnlyCollection<TimeZoneInfo> timeZones = TimeZoneInfo.GetSystemTimeZones();
        /// <summary>
        /// 指定当地时间和其对应的 utc 时间, 得到这个地方的时区（只能精确到时区）
        /// +5: 即东五区
        /// -3: 即西三区
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="utc"></param>
        /// <returns></returns>
        public static TimeZoneInfo GetTimeZone(DateTime loc, DateTime utc)
        {
            var offset = loc - utc;
            return timeZones.First((x) =>
             {
                 return Math.Abs((x.BaseUtcOffset - offset).TotalMinutes) < 10;
             });
        }

        /// <summary>
        /// 根据当前时间，以及指定的时区，返回当地时间
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="timeZone"></param>
        /// <returns></returns>
        public static DateTime ReviseLoc(TimeZoneInfo timeZone)
        {
            return TimeZoneInfo.ConvertTime(DateTime.Now, timeZone);
        }

        public static DateTime ReviseLoc(TimeSpan UtcOffset)
        {
            return DateTime.UtcNow + UtcOffset;
        }

        public static DateTime ReviseLoc(TimeZoneInfo timeZone, DateTime dateTime)
        {
            return TimeZoneInfo.ConvertTime(dateTime, timeZone);
        }

        public static DateTime ReviseLoc(DateTime dateTime, TimeSpan UtcOffset)
        {
            return dateTime.ToUniversalTime() + UtcOffset;
        }

    }
}
