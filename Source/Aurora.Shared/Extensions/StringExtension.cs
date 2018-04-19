// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Aurora.Shared.Extensions
{
    public static class StringExtension
    {
        public static string Truncate(this string value, int length) =>
            (value != null && value.Length > length) ? value.Substring(0, length) : value;

        public static int LevenshteinDistance(this string source, string t)
        {
            return LevenshteinCompute(source, t);
        }
        private static int LevenshteinCompute(string s, string t)
        {
            if (string.IsNullOrEmpty(s))
            {
                if (string.IsNullOrEmpty(t))
                    return 0;
                return t.Length;
            }

            if (string.IsNullOrEmpty(t))
            {
                return s.Length;
            }

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // initialize the top and right of the table to 0, 1, 2, ...
            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 1; j <= m; d[0, j] = j++) ;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    int min1 = d[i - 1, j] + 1;
                    int min2 = d[i, j - 1] + 1;
                    int min3 = d[i - 1, j - 1] + cost;
                    d[i, j] = Math.Min(Math.Min(min1, min2), min3);
                }
            }
            return d[n, m];
        }

        static string[] formats = { @"m\:ss", @"h\:mm\:ss", @"m\:ss\.FFF", @"h\:mm\:ss\.FFF" };

        public static TimeSpan ParseDuration(this string str)
        {
            if (TimeSpan.TryParseExact(str, formats, CultureInfo.InvariantCulture, out TimeSpan t))
            {
                return t;
            }
            if (double.TryParse(str, out double d))
            {
                return TimeSpan.FromSeconds(d);
            }
            return default(TimeSpan);
        }

        public static string Combine(this IEnumerable<string> strArr)
        {
            return string.Join(":|:", strArr);
        }

        public static string ToFirstUpper(this string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }

        public static string Reverse(this string s)
        {
            return new string(s.ToCharArray().Reverse().ToArray());
        }

        public static long HextoDec(this string s)
        {
            var c = s.ToCharArray();
            long res = 0, k = 1;
            for (int i = c.Length - 1; i >= 0; i--)
            {
                res += (Array.IndexOf(LongExtension.HexSet, c[i])) * k;
                k *= 16;
            }
            return res;
        }

        public static bool IsNullorEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        public static bool IsNullorWhiteSpace(this string s)
        {
            return string.IsNullOrWhiteSpace(s);
        }

        public static long OcttoDec(this string s)
        {
            var c = s.ToCharArray();
            long res = 0, k = 1;
            for (int i = c.Length - 1; i >= 0; i--)
            {
                res += (Array.IndexOf(LongExtension.HexSet, c[i])) * k;
                k *= 8;
            }
            return res;
        }

        public static long BintoDec(this string s)
        {
            var c = s.ToCharArray();
            long res = 0, k = 1;
            for (int i = c.Length - 1; i >= 0; i--)
            {
                res += (Array.IndexOf(LongExtension.HexSet, c[i])) * k;
                k *= 2;
            }
            return res;
        }

    }
}
