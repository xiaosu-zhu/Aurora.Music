// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;

namespace Aurora.Music.Core.Tools
{
    public static class TimeSpanFormatter
    {
        public static string GetSongDurationFormat(this TimeSpan t)
        {
            if (t.TotalMinutes < 1)
            {
                return SmartFormat.Smart.Format(Consts.Localizer.GetString("SmartDurationShort"), t.Seconds);
            }
            return SmartFormat.Smart.Format(Consts.Localizer.GetString("SmartDuration"), Math.Floor(t.TotalMinutes), t.Seconds);
        }

        public static string GetAlbumDurationFormat(this TimeSpan t)
        {
            if (t.TotalHours < 1)
            {
                return SmartFormat.Smart.Format(Consts.Localizer.GetString("SmartDuration"), Math.Floor(t.TotalMinutes), t.Seconds);
            }
            return SmartFormat.Smart.Format(Consts.Localizer.GetString("SmartDurationLong"), Math.Floor(t.TotalHours), t.Minutes);
        }
    }
}
