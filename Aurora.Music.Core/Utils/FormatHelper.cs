using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Music.Core.Utils
{
    public static class TimeSpanFormatter
    {
        public static string GetSongDurationFormat(this TimeSpan t)
        {
            if (t.TotalMinutes == 1)
            {
                if (t.Seconds == 1)
                {
                    return "1 min, 1 sec";
                }
                else return $"1 min, {t.Seconds} secs";
            }
            else if (t.Seconds == 1)
            {
                return $"{Math.Floor(t.TotalMinutes)} mins, 1 sec";
            }
            else return $"{Math.Floor(t.TotalMinutes)} mins, {Math.Floor(t.TotalMinutes)} secs";
        }

        public static string GetAlbumDurationFormat(this TimeSpan t)
        {
            if (t.TotalHours == 1)
            {
                if (t.Minutes == 1)
                {
                    return "1 hour, 1 min";
                }
                else return $"1 hour, {t.Minutes} mins";
            }
            else if (t.Seconds == 1)
            {
                return $"{Math.Floor(t.TotalHours)} hours, 1 min";
            }
            else return $"{Math.Floor(t.TotalHours)} hours, {t.Minutes} mins";
        }
    }
}
