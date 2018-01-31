// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;

namespace MathNet.Filtering.Windowing
{
    /// <summary>
    /// Hann window.
    /// </summary>
    /// <remarks>
    /// Named after Julius von Hann.
    /// </remarks>
    public class HannWindow : Window
    {
        /// <summary>
        /// Windowing function generator implementation.
        /// </summary>
        protected unsafe override float[] ComputeWindowCore(int width)
        {
            var w = Numerics.Window.Hann(width);

            float[] res = new float[w.Length];

            fixed (double* dp1 = w)
            {
                fixed (float* fp1 = res)
                {
                    for (int i = 0; i < w.Length; i++)
                    {
                        res[i] = (float)dp1[i];
                    }
                }
            }

            return res;
        }
    }
}
