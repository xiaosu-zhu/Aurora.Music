// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;

namespace MathNet.Filtering.IIR
{
    /// <summary>
    /// Infinite Impulse Response (FIR) Filters need much
    /// less coefficients (and are thus much faster) than
    /// comparable FIR Filters, but are potentially unstable.
    /// IIR Filters are always online and causal. This IIR
    /// Filter implements the canonic Direct Form II structure.
    /// </summary>
    /// <remarks>
    /// System Description: H(z) = (b0 + b1*z^-1 + b2*z^-2) / (1 + a1*z^-1 + a2*z^-2)
    /// </remarks>
    public class OnlineIirFilter : OnlineFilter
    {
        readonly float[] _b;
        readonly float[] _a;
        readonly float[] _bufferX;
        readonly float[] _bufferY;
        readonly int _halfSize;
        int _offset;

        /// <summary>
        /// Infinite Impulse Response (IIR) Filter.
        /// </summary>
        public OnlineIirFilter(float[] coefficients)
        {
            if (null == coefficients)
            {
                throw new ArgumentNullException("coefficients");
            }

            if ((coefficients.Length & 1) != 0)
            {
                throw new ArgumentException("ArgumentEvenNumberOfCoefficients", "coefficients");
            }

            int size = coefficients.Length;
            _halfSize = size >> 1;
            _b = new float[size];
            _a = new float[size];

            for (int i = 0; i < _halfSize; i++)
            {
                _b[i] = _b[_halfSize + i] = coefficients[i];
                _a[i] = _a[_halfSize + i] = coefficients[_halfSize + i];
            }

            _bufferX = new float[size];
            _bufferY = new float[size];
        }

        /// <summary>
        /// Process a single sample.
        /// </summary>
        public override float ProcessSample(float sample)
        {
            _offset = (_offset != 0) ? _offset - 1 : _halfSize - 1;
            _bufferX[_offset] = sample;
            _bufferY[_offset] = 0f;
            float yn = 0f;

            for (int i = 0, j = _halfSize - _offset; i < _halfSize; i++, j++)
            {
                yn += _bufferX[i]*_b[j];
            }

            for (int i = 0, j = _halfSize - _offset; i < _halfSize; i++, j++)
            {
                yn -= _bufferY[i]*_a[j];
            }

            _bufferY[_offset] = yn;
            return yn;
        }

        /// <summary>
        /// Reset internal state (not coefficients!).
        /// </summary>
        public override void Reset()
        {
            for (int i = 0; i < _bufferX.Length; i++)
            {
                _bufferX[i] = 0f;
                _bufferY[i] = 0f;
            }
        }
    }
}
