// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Aurora.Shared.Helpers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Aurora.Shared.Extensions
{
    public static class EnumrableExtesion
    {
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
