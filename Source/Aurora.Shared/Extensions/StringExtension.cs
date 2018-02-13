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
        static string[] formats = { @"m\:ss", @"h\:mm\:ss", @"m\:ss\.FFF", @"h\:mm\:ss\.FFF" };

        public static TimeSpan ParseDuration(this string str)
        {
            return TimeSpan.ParseExact(str, formats, CultureInfo.InvariantCulture);
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
