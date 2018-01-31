// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using MathNet.Filtering.FIR;
using MathNet.Filtering.IIR;
using MathNet.Filtering.Median;

namespace MathNet.Filtering
{
    /// <summary>
    /// Online filters allow processing incoming samples immediately and hence
    /// provide a nearly real-time response with a fixed delay.
    /// </summary>
    public abstract class OnlineFilter : IOnlineFilter
    {
        /// <summary>
        /// Create a filter to remove high frequencies in online processing scenarios.
        /// </summary>
        public static OnlineFilter CreateLowpass(ImpulseResponse mode, float sampleRate, float cutoffRate, int order)
        {
            if (mode == ImpulseResponse.Finite)
            {
                float[] c = FirCoefficients.LowPass(sampleRate, cutoffRate, order >> 1);
                return new OnlineFirFilter(c);
            }

            if (mode == ImpulseResponse.Infinite)
            {
                // TODO: investigate (bandwidth)
                float[] c = IirCoefficients.LowPass(sampleRate, cutoffRate, cutoffRate);
                return new OnlineIirFilter(c);
            }

            throw new ArgumentException("mode");
        }

        /// <summary>
        /// Create a filter to remove high frequencies in online processing scenarios.
        /// </summary>
        public static OnlineFilter CreateLowpass(ImpulseResponse mode, float sampleRate, float cutoffRate)
        {
            return CreateLowpass(
                mode,
                sampleRate,
                cutoffRate,
                mode == ImpulseResponse.Finite ? 64 : 4); // order
        }

        /// <summary>
        /// Create a filter to remove low frequencies in online processing scenarios.
        /// </summary>
        public static OnlineFilter CreateHighpass(ImpulseResponse mode, float sampleRate, float cutoffRate, int order)
        {
            if (mode == ImpulseResponse.Finite)
            {
                float[] c = FirCoefficients.HighPass(sampleRate, cutoffRate, order >> 1);
                return new OnlineFirFilter(c);
            }

            if (mode == ImpulseResponse.Infinite)
            {
                // TODO: investigate (bandwidth)
                float[] c = IirCoefficients.HighPass(sampleRate, cutoffRate, cutoffRate);
                return new OnlineIirFilter(c);
            }

            throw new ArgumentException("mode");
        }

        /// <summary>
        /// Create a filter to remove low frequencies in online processing scenarios.
        /// </summary>
        public static OnlineFilter CreateHighpass(ImpulseResponse mode, float sampleRate, float cutoffRate)
        {
            return CreateHighpass(
                mode,
                sampleRate,
                cutoffRate,
                mode == ImpulseResponse.Finite ? 64 : 4); // order
        }

        /// <summary>
        /// Create a filter to remove low and high frequencies in online processing scenarios.
        /// </summary>
        public static OnlineFilter CreateBandpass(ImpulseResponse mode, float sampleRate, float cutoffLowRate, float cutoffHighRate, int order)
        {
            if (mode == ImpulseResponse.Finite)
            {
                float[] c = FirCoefficients.BandPass(sampleRate, cutoffLowRate, cutoffHighRate, order >> 1);
                return new OnlineFirFilter(c);
            }

            if (mode == ImpulseResponse.Infinite)
            {
                float[] c = IirCoefficients.BandPass(sampleRate, cutoffLowRate, cutoffHighRate);
                return new OnlineIirFilter(c);
            }

            throw new ArgumentException("mode");
        }

        /// <summary>
        /// Create a filter to remove low and high frequencies in online processing scenarios.
        /// </summary>
        public static OnlineFilter CreateBandpass(ImpulseResponse mode, float sampleRate, float cutoffLowRate, float cutoffHighRate)
        {
            return CreateBandpass(
                mode,
                sampleRate,
                cutoffLowRate,
                cutoffHighRate,
                mode == ImpulseResponse.Finite ? 64 : 4); // order
        }

        /// <summary>
        /// Create a filter to remove middle (all but low and high) frequencies in online processing scenarios.
        /// </summary>
        public static OnlineFilter CreateBandstop(ImpulseResponse mode, float sampleRate, float cutoffLowRate, float cutoffHighRate, int order)
        {
            if (mode == ImpulseResponse.Finite)
            {
                float[] c = FirCoefficients.BandStop(sampleRate, cutoffLowRate, cutoffHighRate, order >> 1);
                return new OnlineFirFilter(c);
            }

            if (mode == ImpulseResponse.Infinite)
            {
                float[] c = IirCoefficients.BandStop(sampleRate, cutoffLowRate, cutoffHighRate);
                return new OnlineIirFilter(c);
            }

            throw new ArgumentException("mode");
        }

        /// <summary>
        /// Create a filter to remove middle (all but low and high) frequencies in online processing scenarios.
        /// </summary>
        public static OnlineFilter CreateBandstop(ImpulseResponse mode, float sampleRate, float cutoffLowRate, float cutoffHighRate)
        {
            return CreateBandstop(
                mode,
                sampleRate,
                cutoffLowRate,
                cutoffHighRate,
                mode == ImpulseResponse.Finite ? 64 : 4); // order
        }

        /// <summary>
        /// Create a filter to remove noise in online processing scenarios.
        /// </summary>
        /// <param name="order">
        /// Window Size, should be odd. A larger number results in a smoother
        /// response but also in a longer delay.
        /// </param>
        /// <remarks>The de-noise filter is implemented as an unweighted median filter.</remarks>
        public static OnlineFilter CreateDenoise(int order)
        {
            return new OnlineMedianFilter(order);
        }

        /// <summary>
        /// Create a filter to remove noise in online processing scenarios.
        /// </summary>
        /// <remarks>The de-noise filter is implemented as an unweighted median filter.</remarks>
        public static OnlineFilter CreateDenoise()
        {
            return CreateDenoise(7);
        }

        /// <summary>
        /// Process a single sample.
        /// </summary>
        public abstract float ProcessSample(float sample);

        /// <summary>
        /// Reset internal state (not coefficients!).
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Process a sequence of sample.
        /// </summary>
        public virtual float[] ProcessSamples(float[] samples)
        {
            if (null == samples)
            {
                return null;
            }

            float[] ret = new float[samples.Length];
            for (int i = 0; i < samples.Length; i++)
            {
                ret[i] = ProcessSample(samples[i]);
            }

            return ret;
        }

        public unsafe virtual void ProcessSamples(ref float* samples, int capacity)
        {
            if (null == samples)
            {
                throw new Exception();
            }


            for (int i = 0; i < capacity; i++)
            {
                samples[i] = ProcessSample(samples[i]);
            }

        }
    }
}
