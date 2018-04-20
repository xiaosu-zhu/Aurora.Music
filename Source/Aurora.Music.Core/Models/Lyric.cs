// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Aurora.Music.Core.Models
{
    public class Lyric : List<KeyValuePair<TimeSpan, string>>
    {
        private LrcParser.Lyric lyric;
        public TimeSpan Offset { get; set; }

        public Lyric(LrcParser.Lyric l)
        {
            if (l == null)
            {
                return;
            }
            lyric = l;
            if (l.AddtionalInfo != null)
                Add(new KeyValuePair<TimeSpan, string>(TimeSpan.Zero, string.Join(Environment.NewLine, l.AddtionalInfo.Select(x => $"{x.Key}: {x.Value}"))));
            AddRange(l.Slices.Select(x => new KeyValuePair<TimeSpan, string>(x.Offset, x.Content)));
        }
    }
}
