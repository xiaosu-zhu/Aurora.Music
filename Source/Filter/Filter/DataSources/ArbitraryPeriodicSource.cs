// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using MathNet.Filtering.Channel;

namespace MathNet.Filtering.DataSources
{
    /// <summary>
    /// Precomputed periodic sample source
    /// </summary>
    public class ArbitraryPeriodicSource : IChannelSource
    {
        readonly float[] _samples;
        readonly int _sampleCount;
        readonly int _delay;
        int _nextIndex;

        /// <summary>
        /// Create a new precomputed periodic sample source
        /// </summary>
        public ArbitraryPeriodicSource(float[] samples, int indexOffset, int delay)
        {
            if (null == samples)
            {
                throw new ArgumentNullException("samples");
            }

            if (0 == samples.Length)
            {
                throw new ArgumentOutOfRangeException("samples");
            }

            if (indexOffset < 0 || indexOffset >= samples.Length)
            {
                throw new ArgumentOutOfRangeException("indexOffset");
            }

            _samples = samples;
            _sampleCount = samples.Length;
            _delay = delay;

            int effectiveDelay = delay%_sampleCount;
            _nextIndex = (indexOffset - effectiveDelay + _sampleCount)%_sampleCount;
        }

        /// <summary>
        /// Create a new precomputed periodic sample source
        /// </summary>
        public ArbitraryPeriodicSource(float[] samples)
            : this(samples, 0, 0)
        {
        }

        /// <summary>
        /// Computes and returns the next sample.
        /// </summary>
        public float ReadNextSample()
        {
            float sample = _samples[_nextIndex];
            _nextIndex = (_nextIndex + 1)%_sampleCount;
            return sample;
        }

        /// <summary>
        /// Sample delay of this source in relation to the whole system.
        /// </summary>
        public int Delay
        {
            get { return _delay; }
        }
    }
}
