// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
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

        public static implicit operator string(Lyric l)
        {
            return l.ToString();
        }

        public override string ToString()
        {
            var lrc = new LrcParser.Lyric();

            int i = -1;

            if (lyric.AddtionalInfo != null)
            {
                var item = Find(a => a.Value.StartsWith(lyric.AddtionalInfo.First().Key));

                Sort((a, b) => a.Key.CompareTo(b.Key));
                i = IndexOf(item);

                lrc.AddtionalInfo = lyric.AddtionalInfo;
            }
            else
            {
                Sort((a, b) => a.Key.CompareTo(b.Key));
            }
            lrc.Slices = new List<LrcParser.Slice>();
            for (int j = 0; j < Count; j++)
            {
                if (i == j)
                    continue;
                lrc.Slices.Add(new LrcParser.Slice()
                {
                    Content = this[j].Value,
                    Offset = this[j].Key
                });
            }
            lrc.Offset = Offset;
            return LrcParser.Parser.Create(lrc);
        }
    }
}
