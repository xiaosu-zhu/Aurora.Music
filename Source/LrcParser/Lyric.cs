using System;
using System.Collections.Generic;
using System.Linq;

namespace LrcParser
{
    public class Lyric
    {
        internal Lyric(IOrderedEnumerable<Slice> orderedEnumerable, List<KeyValuePair<string, string>> enumerable)
        {
            Slices = orderedEnumerable.ToList();
            AddtionalInfo = enumerable;
            foreach (var item in AddtionalInfo)
            {
                if (item.Key.Equals("offset", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (double.TryParse(item.Value, out double i))
                    {
                        Offset = TimeSpan.FromMilliseconds(i);
                        AddtionalInfo.Remove(item);
                        break;
                    }
                }
            }
        }

        public Lyric() { }

        public Lyric(string s, TimeSpan duration)
        {
            var sl = s.Split('\n');

            Slices = new List<Slice>(sl.Length);
            for (int i = 0; i < sl.Length; i++)
            {
                Slices.Add(new Slice
                {
                    Offset = TimeSpan.FromMilliseconds(duration.TotalMilliseconds * i / sl.Length),
                    Content = sl[i],
                });
            }
            AddtionalInfo = null;
        }

        public List<Slice> Slices { get; set; }
        public List<KeyValuePair<string, string>> AddtionalInfo { get; set; }

        public TimeSpan Offset { get; set; }
    }

    public class Slice
    {
        public TimeSpan Offset { get; set; }
        public string Content { get; set; }
    }
}
