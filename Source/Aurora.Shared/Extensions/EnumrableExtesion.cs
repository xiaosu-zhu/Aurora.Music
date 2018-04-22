// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Shared.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Aurora.Shared.Extensions
{
    public static class EnumrableExtesion
    {
        public static IEnumerable<TSource> SortLike<TSource, TKey>(this ICollection<TSource> source,
                                            IEnumerable<TKey> sortOrder)
        {
            var cloned = sortOrder.ToArray();
            var sourceArr = source.ToArray();
            Array.Sort(cloned, sourceArr);
            return sourceArr;
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Tools.Random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static bool IsNullorEmpty(this IEnumerable enumerator)
        {
            var erator = enumerator.GetEnumerator();
            erator.MoveNext();
            return (enumerator == null || erator.Current == null);
        }

        public static bool IsNullorEmpty<T>(this IEnumerable<T> enumerator)
        {
            return (enumerator == null || enumerator.Count() == 0);
        }

        public static string SelectRandomString(this string[] s)
        {
            if (!s.IsNullorEmpty())
            {
                var index = Tools.Random.Next(s.Length);
                return s[index];
            }
            return null;
        }
    }
}
