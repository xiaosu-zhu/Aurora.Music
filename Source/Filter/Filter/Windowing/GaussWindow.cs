// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;

namespace MathNet.Filtering.Windowing
{
    /// <summary>
    /// Gauss window.
    /// </summary>
    public class GaussWindow : Window
    {
        float _sigma;

        /// <summary>
        /// Create a new gauss window.
        /// </summary>
        public GaussWindow(float sigma)
        {
            _sigma = sigma;
        }

        /// <summary>
        /// Sigma (standard distribution of the Gaussian distribution).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public float Sigma
        {
            get { return _sigma; }
            set
            {
                if (0.0 > value || 0.5 < value)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                _sigma = value;
                Reset();
            }
        }

        /// <summary>
        /// Windowing function generator implementation.
        /// </summary>
        protected unsafe override float[] ComputeWindowCore(int width)
        {
            var w = Numerics.Window.Gauss(width, _sigma);

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
