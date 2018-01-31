// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System.Linq;
using MathNet.Numerics.Statistics;

namespace MathNet.Filtering.Median
{
    /// <summary>
    /// Median-Filters are non-linear filters, returning
    /// the median of a sample window as output. Median-Filters
    /// perform well for de-noise applications where it's
    /// important to not loose sharp steps/edges.
    /// </summary>
    public class OnlineMedianFilter : OnlineFilter
    {
        readonly float[] _buffer;
        int _offset;
        bool _bufferFull;

        /// <summary>
        /// Create a Median Filter
        /// </summary>
        public OnlineMedianFilter(int windowSize)
        {
            _buffer = new float[windowSize];
        }

        /// <summary>
        /// Process a single sample.
        /// </summary>
        public override float ProcessSample(float sample)
        {
            _buffer[_offset = (_offset == 0) ? _buffer.Length - 1 : _offset - 1] = sample;
            _bufferFull |= _offset == 0;

            var data = _bufferFull ? _buffer : _buffer.Skip(_offset);
            return data.Median();
        }

        /// <summary>
        /// Reset internal state.
        /// </summary>
        public override void Reset()
        {
            _offset = 0;
            _bufferFull = false;
        }
    }
}
