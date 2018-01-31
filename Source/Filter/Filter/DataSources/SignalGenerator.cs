// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;

namespace MathNet.Filtering.DataSources
{
    /// <summary>
    /// Generators for sinusoidal and theoretical signal vectors.
    /// </summary>
    public static class SignalGenerator
    {
        /// <summary>
        /// Create a Sine Signal Sample Vector.
        /// </summary>
        /// <param name="samplingRate">Samples per unit.</param>
        /// <param name="frequency">Frequency in samples per unit.</param>
        /// <param name="phase">Optional phase offset.</param>
        /// <param name="amplitude">The maximal reached peak.</param>
        /// <param name="length">The count of samples to generate.</param>
        public static float[] Sine(float samplingRate, float frequency, float phase, float amplitude, int length)
        {
            float[] data = new float[length];
            float step = frequency / samplingRate * 2f * (float)Math.PI;

            for (int i = 0; i < length; i++)
            {
                data[i] = amplitude * (float)Math.Sin(phase + i * step);
            }

            return data;
        }

        /// <summary>
        /// Create a Heaviside Step Signal Sample Vector.
        /// </summary>
        /// <param name="offset">Offset to the time axis. Zero or positive.</param>
        /// <param name="amplitude">The maximal reached peak.</param>
        /// <param name="length">The count of samples to generate.</param>
        public static float[] Step(int offset, float amplitude, int length)
        {
            var data = new float[length];
            int cursor;

            for (cursor = 0; cursor < offset && cursor < length; cursor++)
            {
                data[cursor] = 0f;
            }

            for (; cursor < length; cursor++)
            {
                data[cursor] = amplitude;
            }

            return data;
        }

        /// <summary>
        /// Create a Dirac Delta Impulse Signal Sample Vector.
        /// </summary>
        /// <param name="offset">Offset to the time axis. Zero or positive.</param>
        /// <param name="frequency">impulse sequence frequency. -1 for single impulse only.</param>
        /// <param name="amplitude">The maximal reached peak.</param>
        /// <param name="length">The count of samples to generate.</param>
        public static float[] Impulse(int offset, int frequency, float amplitude, int length)
        {
            var data = new float[length];

            if (frequency <= 0)
            {
                data[offset] = amplitude;
            }
            else
            {
                while (offset < length)
                {
                    data[offset] = amplitude;
                    offset += frequency;
                }
            }

            return data;
        }
    }
}
