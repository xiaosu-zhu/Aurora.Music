using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LrcParser;

namespace Aurora.Music.Core.Models
{
    public class Lyric : List<KeyValuePair<TimeSpan, string>>
    {
        private LrcParser.Lyric lyric;
        public TimeSpan Offset { get; set; }

        public Lyric(LrcParser.Lyric l)
        {
            this.lyric = l;
            Add(new KeyValuePair<TimeSpan, string>(TimeSpan.Zero, string.Join(Environment.NewLine, l.AddtionalInfo.Select(x => $"{x.Key}: {x.Value}"))));
            AddRange(l.Slices.Select(x => new KeyValuePair<TimeSpan, string>(x.Offset, x.Contet)));
        }
    }
}
