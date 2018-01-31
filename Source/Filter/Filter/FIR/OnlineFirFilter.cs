// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System.Collections.Generic;

namespace MathNet.Filtering.FIR
{
    /// <summary>
    /// Finite Impulse Response (FIR) Filters are based on
    /// Fourier series and implemented using a discrete
    /// convolution equation. FIR Filters are always
    /// online, stable and causal.
    /// </summary>
    /// <remarks>
    /// System Description: H(z) = a0 + a1*z^-1 + a2*z^-2 + ...
    /// </remarks>
    public class OnlineFirFilter : OnlineFilter
    {
        readonly float[] _coefficients;
        readonly float[] _buffer;
        int _offset;
        readonly int _size;

        /// <summary>
        /// Finite Impulse Response (FIR) Filter.
        /// </summary>
        public OnlineFirFilter(IList<float> coefficients)
        {
            _size = coefficients.Count;
            _buffer = new float[_size];
            _coefficients = new float[_size << 1];
            for (int i = 0; i < _size; i++)
            {
                _coefficients[i] = _coefficients[_size + i] = coefficients[i];
            }
        }

        /// <summary>
        /// Process a single sample.
        /// </summary>
        public override float ProcessSample(float sample)
        {
            _offset = (_offset != 0) ? _offset - 1 : _size - 1;
            _buffer[_offset] = sample;

            float acc = 0;
            for (int i = 0, j = _size - _offset; i < _size; i++, j++)
            {
                acc += _buffer[i]*_coefficients[j];
            }

            return acc;
        }

        /// <summary>
        /// Reset internal state (not coefficients!).
        /// </summary>
        public override void Reset()
        {
            for (int i = 0; i < _buffer.Length; i++)
            {
                _buffer[i] = 0f;
            }
        }
    }
}
