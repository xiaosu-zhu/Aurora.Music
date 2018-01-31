// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;

namespace MathNet.Filtering.FIR
{
    /// <summary>
    /// FirCoefficients provides basic coefficient evaluation
    /// algorithms for the four most important filter types for
    /// Finite Impulse Response (FIR) Filters.
    ///
    /// Default filter order estimation:
    /// transition bandwidth is 25% of the lower passband edge,
    /// but not lower than 2 Hz, where possible (for bandpass,
    /// highpass, and bandstop) and distance from passband edge
    /// to critical frequency (DC, Nyquist) otherwise.
    /// </summary>
    public static class FirCoefficients
    {
        /// <summary>
        /// Calculates FIR LowPass Filter Coefficients.
        /// </summary>
        /// <param name="samplingRate">Samples per unit.</param>
        /// <param name="cutoff">Cutoff frequency in samples per unit.</param>
        /// <param name="halforder">half-order Q, so that Order N = 2*Q+1. 0 for default order.</param>
        /// <returns>The calculated filter coefficients.</returns>
        public static float[] LowPass(float samplingRate, float cutoff, int halforder = 0)
        {
            float nu = 2f * cutoff / samplingRate; // normalized frequency

            // Default filter order
            if (halforder == 0)
            {
                const float TRANSWINDRATIO = 0.25f;
                float maxDf = samplingRate / 2 - cutoff;
                float df = (cutoff * TRANSWINDRATIO > 2) ? cutoff * TRANSWINDRATIO : 2;
                df = (df < maxDf) ? df : maxDf;
                halforder = (int)Math.Ceiling(3.3 / (df / samplingRate) / 2);
            }

            int order = 2 * halforder + 1;
            var c = new float[order];
            c[halforder] = nu;

            for (int i = 0, n = halforder; i < halforder; i++, n--)
            {
                float npi = n * (float)Math.PI;
                c[i] = (float)Math.Sin(npi * nu) / npi;
                c[n + halforder] = c[i];
            }

            return c;
        }

        /// <summary>
        /// Calculates FIR HighPass Filter Coefficients.
        /// </summary>
        /// <param name="samplingRate">Samples per unit.</param>
        /// <param name="cutoff">Cutoff frequency in samples per unit.</param>
        /// <param name="halforder">half-order Q, so that Order N = 2*Q+1. 0 for default order.</param>
        /// <returns>The calculated filter coefficients.</returns>
        public static float[] HighPass(float samplingRate, float cutoff, int halforder = 0)
        {
            float nu = 2f * cutoff / samplingRate; // normalized frequency

            // Default filter order
            if (halforder == 0)
            {
                const float TRANSWINDRATIO = 0.25f;
                float maxDf = cutoff;
                float df = (maxDf * TRANSWINDRATIO > 2) ? maxDf * TRANSWINDRATIO : 2;
                df = (df < maxDf) ? df : maxDf;
                halforder = (int)Math.Ceiling(3.3 / (df / samplingRate) / 2);
            }

            int order = 2 * halforder + 1;
            var c = new float[order];
            c[halforder] = 1 - nu;

            for (int i = 0, n = halforder; i < halforder; i++, n--)
            {
                float npi = n * (float)Math.PI;
                c[i] = -(float)Math.Sin(npi * nu) / npi;
                c[n + halforder] = c[i];
            }

            return c;
        }

        /// <summary>
        /// Calculates FIR Bandpass Filter Coefficients.
        /// </summary>
        /// <param name="samplingRate">Samples per unit.</param>
        /// <param name="cutoffLow">Low Cutoff frequency in samples per unit.</param>
        /// <param name="cutoffHigh">High Cutoff frequency in samples per unit.</param>
        /// <param name="halforder">half-order Q, so that Order N = 2*Q+1. 0 for default order.</param>
        /// <returns>The calculated filter coefficients.</returns>
        public static float[] BandPass(float samplingRate, float cutoffLow, float cutoffHigh, int halforder = 0)
        {
            float nu1 = 2f * cutoffLow / samplingRate; // normalized low frequency
            float nu2 = 2f * cutoffHigh / samplingRate; // normalized high frequency

            // Default filter order
            if (halforder == 0)
            {
                const float TRANSWINDRATIO = 0.25f;
                float maxDf = (cutoffLow < samplingRate / 2 - cutoffHigh) ? cutoffLow : samplingRate / 2 - cutoffHigh;
                float df = (cutoffLow * TRANSWINDRATIO > 2) ? cutoffLow * TRANSWINDRATIO : 2;
                df = (df < maxDf) ? df : maxDf;
                halforder = (int)Math.Ceiling(3.3 / (df / samplingRate) / 2);
            }

            int order = 2 * halforder + 1;
            var c = new float[order];
            c[halforder] = nu2 - nu1;

            for (int i = 0, n = halforder; i < halforder; i++, n--)
            {
                float npi = n * (float)Math.PI;
                c[i] = (float)(Math.Sin(npi * nu2) - Math.Sin(npi * nu1)) / npi;
                c[n + halforder] = c[i];
            }

            return c;
        }

        /// <summary>
        /// Calculates FIR Bandstop Filter Coefficients.
        /// </summary>
        /// <param name="samplingRate">Samples per unit.</param>
        /// <param name="cutoffLow">Low Cutoff frequency in samples per unit.</param>
        /// <param name="cutoffHigh">High Cutoff frequency in samples per unit.</param>
        /// <param name="halforder">half-order Q, so that Order N = 2*Q+1. 0 for default order.</param>
        /// <returns>The calculated filter coefficients.</returns>
        public static float[] BandStop(float samplingRate, float cutoffLow, float cutoffHigh, int halforder = 0)
        {
            float nu1 = 2f * cutoffLow / samplingRate; // normalized low frequency
            float nu2 = 2f * cutoffHigh / samplingRate; // normalized high frequency

            // Default filter order
            if (halforder == 0)
            {
                const float TRANSWINDRATIO = 0.25f;
                float maxDf = (cutoffHigh - cutoffLow) / 2;
                float df = (maxDf * TRANSWINDRATIO > 2) ? maxDf * TRANSWINDRATIO : 2;
                df = (df < maxDf) ? df : maxDf;
                halforder = (int)Math.Ceiling(3.3 / (df / samplingRate) / 2);
            }

            int order = 2 * halforder + 1;
            var c = new float[order];
            c[halforder] = 1 - (nu2 - nu1);

            for (int i = 0, n = halforder; i < halforder; i++, n--)
            {
                float npi = n * (float)Math.PI;
                c[i] = (float)(Math.Sin(npi * nu1) - Math.Sin(npi * nu2)) / npi;
                c[n + halforder] = c[i];
            }

            return c;
        }
    }
}
