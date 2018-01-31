// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;

namespace MathNet.Filtering.IIR
{
    /// <summary>
    /// IirCoefficients provides basic coefficient evaluation
    /// algorithms for the four most important filter types for
    /// Infinite Impulse Response (IIR) Filters.
    /// </summary>
    public static class IirCoefficients
    {
        /// <summary>
        /// Calculates IIR LowPass Filter Coefficients.
        /// </summary>
        /// <param name="samplingRate">Samples per unit.</param>
        /// <param name="cutoff">Cutoff frequency in samples per unit.</param>
        /// <param name="width">bandwidth in samples per unit.</param>
        /// <returns>The calculated filter coefficients.</returns>
        public static float[] LowPass(float samplingRate, float cutoff, float width)
        {
            float beta, gamma, theta;

            BetaGamma(
                out beta,
                out gamma,
                out theta,
                samplingRate,
                cutoff,
                0f, // lowHalfPower
                width); // highHalfPower

            return BuildCoefficients(
                beta,
                gamma,
                (0.5f + beta - gamma) * 0.25f, // alpha
                2, // mu
                1); // sigma
        }

        /// <summary>
        /// Calculates IIR HighPass Filter Coefficients.
        /// </summary>
        /// <param name="samplingRate">Samples per unit.</param>
        /// <param name="cutoff">Cutoff frequency in samples per unit.</param>
        /// <param name="width">bandwidth in samples per unit.</param>
        /// <returns>The calculated filter coefficients.</returns>
        public static float[] HighPass(float samplingRate, float cutoff, float width)
        {
            float beta, gamma, theta;

            BetaGamma(
                out beta,
                out gamma,
                out theta,
                samplingRate,
                cutoff,
                0f, // lowHalfPower
                width); // highHalfPower

            return BuildCoefficients(
                beta,
                gamma,
                (0.5f + beta + gamma) * 0.25f, // alpha
                -2, // mu
                1); // sigmas
        }

        /// <summary>
        /// Calculates IIR Bandpass Filter Coefficients.
        /// </summary>
        /// <param name="samplingRate">Samples per unit.</param>
        /// <param name="cutoffLow">Low Cutoff frequency in samples per unit.</param>
        /// <param name="cutoffHigh">High Cutoff frequency in samples per unit.</param>
        /// <returns>The calculated filter coefficients.</returns>
        public static float[] BandPass(float samplingRate, float cutoffLow, float cutoffHigh)
        {
            float beta, gamma, theta;

            BetaGamma(
                out beta,
                out gamma,
                out theta,
                samplingRate,
                (cutoffLow + cutoffHigh) * 0.5f, // cutoff
                cutoffLow, // lowHalfPower
                cutoffHigh); // highHalfPower

            return BuildCoefficients(
                beta,
                gamma,
                (0.5f - beta) * 0.5f, // alpha
                0, // mu
                -1); // sigma
        }

        /// <summary>
        /// Calculates IIR Bandstop Filter Coefficients.
        /// </summary>
        /// <param name="samplingRate">Samples per unit.</param>
        /// <param name="cutoffLow">Low Cutoff frequency in samples per unit.</param>
        /// <param name="cutoffHigh">High Cutoff frequency in samples per unit.</param>
        /// <returns>The calculated filter coefficients.</returns>
        public static float[] BandStop(float samplingRate, float cutoffLow, float cutoffHigh)
        {
            float beta, gamma, theta;

            BetaGamma(
                out beta,
                out gamma,
                out theta,
                samplingRate,
                (cutoffLow + cutoffHigh) * 0.5f, // cutoff
                cutoffLow, // lowHalfPower
                cutoffHigh); // highHalfPower

            return BuildCoefficients(
                beta,
                gamma,
                (0.5f + beta) * 0.5f, // alpha
                -2f * (float)Math.Cos(theta), // mu
                1); // sigma
        }

        static float[] BuildCoefficients(float beta, float gamma, float alpha, float mu, float sigma)
        {
            return new[]
            {
                2f*alpha,
                2f*gamma,
                -2f*beta,
                1f,
                mu,
                sigma
            };
        }

        static void BetaGamma(out float beta, out float gamma, out float theta, float sampling, float cutoff, float lowHalfPower, float highHalfPower)
        {
            float tan = (float)Math.Tan(Math.PI * (highHalfPower - lowHalfPower) / sampling);
            beta = 0.5f * (1 - tan) / (1 + tan);
            theta = 2f * (float)Math.PI * cutoff / sampling;
            gamma = (0.5f + beta) * (float)Math.Cos(theta);
        }
    }
}