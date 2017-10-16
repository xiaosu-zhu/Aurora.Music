// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Aurora.Shared.Helpers
{
    /// <summary>
    /// 包含各种常用静态方法的封装
    /// </summary>
    public static class Tools
    {
        /// <summary>
        /// 把枚举值放入一个列表中
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> GetEnumAsList<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>().ToList();
        }

        /// <summary>
        /// 角度转弧度
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static float DegreesToRadians(float angle)
        {
            return angle * (float)Math.PI / 180;
        }

        /// <summary>
        /// 弧度转角度
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static float RadiansToDegrees(float angle)
        {
            return angle * 180 / (float)Math.PI;
        }

        public static Random Random { get; } = new Random(Guid.NewGuid().GetHashCode());

        /// <summary>
        /// 得到一个区间内的随机数
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float RandomBetween(float min, float max)
        {
            return min == max ? min : min + (float)Random.NextDouble() * (max - min);
        }

        public static bool RandomBool()
        {
            return Random.NextDouble() > 0.5;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trueRate">0 到 100 间的数，表示返回真的比例</param>
        /// <returns></returns>
        public static bool RandomBool(int trueRate)
        {
            return Random.NextDouble() <= ((double)trueRate / 100);
        }

        /// <summary>
        /// 近似字符串匹配
        /// </summary>
        /// <param name="s">源</param>
        /// <param name="t">目标</param>
        /// <returns>字符串距离，越小越匹配</returns>
        public static int LevenshteinDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }
    }
}
