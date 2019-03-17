// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Shared.Helpers;
using System.Text;

namespace Aurora.Shared.Extensions
{
    public static class LongExtension
    {
        public static readonly char[] HexSet = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        public static string DectoBin(this long dec)
        {
            var sb = new StringBuilder();
            while (dec != 0)
            {
                var k = dec % 2;
                dec /= 2;
                sb.Append(HexSet[k]);
            }
            return sb.ToString().Reverse();
        }

        public static string DectoOct(this long dec)
        {
            var sb = new StringBuilder();
            while (dec != 0)
            {
                var k = dec % 8;
                dec /= 8;
                sb.Append(HexSet[k]);
            }
            return sb.ToString().Reverse();
        }

        public static string DectoHex(this long dec)
        {
            return dec.ToString("X", CultureInfoHelper.CurrentCulture);
        }
        public static string ToHexString(this ulong dec)
        {
            return dec.ToString("X", CultureInfoHelper.CurrentCulture);
        }
    }

    public static class IntExtension
    {
        public static string ToHexString(this int dec)
        {
            return dec.ToString("X", CultureInfoHelper.CurrentCulture);
        }

        public static string ToString(this int[][] k)
        {
            var sb = new StringBuilder();
            int i = 0;
            foreach (var row in k)
            {
                sb.Append("[" + i + "]: ");
                foreach (var item in row)
                {
                    sb.Append(item.ToString() + ", ");
                }
                sb.Remove(sb.Length - 2, 2);
                sb.Append("\r\n");
                i++;
            }
            return sb.ToString();
        }
    }
}
