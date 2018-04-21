using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LrcParser
{
    public class Parser
    {
        private static List<KeyValuePair<string, string>> ParsePrefix(string lrc)
        {
            var regex = new Regex(@"\[[^\[\]]+]");
            var list = new List<KeyValuePair<string, string>>();
            var m = regex.Match(lrc);
            int i = 0, j = 0;
            while (j < lrc.Length)
            {
                var val = m.Value;
                var len = m.Length;
                i = m.Index;
                m = m.NextMatch();
                j = m.Success ? m.Index : lrc.Length;
                list.Add(new KeyValuePair<string, string>(val, m.Success ? lrc.Substring(i + len, (j - i - len)) : lrc.Substring(i + len)));
            }
            return list;
        }

        public static Lyric Parse(string lrc, TimeSpan duration)
        {
            if (lrc == null)
            {
                return null;
            }
            var slices = ParsePrefix(lrc);
            if (slices.Count > 0 && !string.IsNullOrEmpty(slices[0].Key))
            {
                return new Lyric(CreateSlice(slices).OrderBy(x => x.Offset), CreateDescription(slices));
            }
            return new Lyric(lrc, duration);
        }

        private static List<KeyValuePair<string, string>> CreateDescription(IEnumerable<KeyValuePair<string, string>> slices)
        {
            var descriptionwithbrace = new Regex(@"\[[a-zA-Z]+:[^\[\]]+\]");
            var list = new List<KeyValuePair<string, string>>();
            foreach (var slice in slices)
            {
                var desc = descriptionwithbrace.Match(slice.Key);
                if (desc.Success)
                {
                    var sub = desc.Value.Substring(1, desc.Value.Length - 2);
                    var d = sub.IndexOf(':');
                    list.Add(new KeyValuePair<string, string>(sub.Substring(0, d), sub.Substring(d + 1)));
                }
            }
            return list;
        }

        private static string[] acceptedTimeFormats = new[]
        {
            @"m\:s",
            @"m\:s\.f",
            @"m\:s\.ff",
            @"m\:s\.fff",
            @"m\:s\.ffff",
        };

        public static IEnumerable<Slice> CreateSlice(IList<KeyValuePair<string, string>> slices)
        {
            var timewithbrace = new Regex(@"\[\d+:\d+(|.\d+)\]");
            var list = new List<Slice>();
            for (int i = 0; i < slices.Count; i++)
            {
                var time = timewithbrace.Match(slices[i].Key);
                if (time.Success)
                {
                    var t = time.Value.Substring(1, time.Value.Length - 2);
                    if (TimeSpan.TryParseExact(t, acceptedTimeFormats, null, out var p))
                    {
                        for (int j = i; j < slices.Count; j++)
                        {
                            if (!string.IsNullOrEmpty(slices[j].Value))
                            {
                                list.Add(new Slice()
                                {
                                    Offset = p,
                                    Content = slices[j].Value.Trim(" \r\n".ToCharArray())
                                });
                                break;
                            }
                        }
                    }
                }
            }
            return list;
        }

        public static string Create(Lyric lrc)
        {
            var sb = new StringBuilder();
            if (lrc.AddtionalInfo != null)
            {
                foreach (var item in lrc.AddtionalInfo)
                {
                    sb.Append($"[{item.Key}: {item.Value}]");
                    sb.Append(Environment.NewLine);
                }
            }
            if (lrc.Offset != TimeSpan.Zero)
            {
                if (lrc.Offset.TotalMilliseconds > 0)
                {
                    sb.Append($"[offset: +{ lrc.Offset.TotalMilliseconds.ToString("0.####")}]");
                }
                else
                {
                    sb.Append($"[offset: { lrc.Offset.TotalMilliseconds.ToString("0.####")}]");
                }
                sb.Append(Environment.NewLine);
            }
            foreach (var l in lrc.Slices)
            {
                sb.Append($"[{l.Offset.ToString(@"m\:ss\.ff")}]{l.Content}");
                sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }
    }
}
