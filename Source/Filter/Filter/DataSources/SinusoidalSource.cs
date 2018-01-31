// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using MathNet.Filtering.Channel;
using MathNet.Numerics;

namespace MathNet.Filtering.DataSources
{
    /// <summary>
    /// Sinus sample source.
    /// </summary>
    public class SinusoidalSource : IChannelSource
    {
        readonly int _delay;
        readonly float _amplitude;
        readonly float _mean;
        readonly float _phaseStep;
        float _nextPhase;

        /// <summary>
        /// Create a new on-demand sinus sample source with the given parameters.
        /// </summary>
        public SinusoidalSource(float samplingRate, float frequency, float amplitude, float phase, float mean, int delay)
        {
            _delay = delay;
            _mean = mean;
            _amplitude = amplitude;
            _phaseStep = frequency / samplingRate * (float)Constants.Pi2;
            _nextPhase = phase - delay * _phaseStep;
        }

        /// <summary>
        /// Create a new on-demand sinus sample source with the given parameters an zero phase and mean.
        /// </summary>
        public SinusoidalSource(float samplingRate, float frequency, float amplitude)
            : this(samplingRate, frequency, amplitude, 0.0f, 0.0f, 0)
        {
        }

        /// <summary>
        /// Creates a precomputed sinus sample source with the given parameters and zero mean.
        /// </summary>
        public static IChannelSource Precompute(int samplesPerPeriod, float amplitude, float phase)
        {
            float[] samples = SignalGenerator.Sine(
                samplesPerPeriod, // samplingRate
                1.0f, // frequency
                phase,
                amplitude,
                samplesPerPeriod); // length

            return new ArbitraryPeriodicSource(samples);
        }

        /// <summary>
        /// Creates a precomputed sinus sample source with the given parameters and zero phase and mean.
        /// </summary>
        public static IChannelSource Precompute(int samplesPerPeriod, float amplitude)
        {
            return Precompute(samplesPerPeriod, amplitude, 0.0f);
        }

        /// <summary>
        /// Computes and returns the next sample.
        /// </summary>
        public float ReadNextSample()
        {
            float sample = _mean + _amplitude * (float)Math.Sin(_nextPhase);
            _nextPhase += _phaseStep;
            float pi2 = (float)Constants.Pi2;

            if (_nextPhase > pi2)
            {
                _nextPhase -= pi2;
            }

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
