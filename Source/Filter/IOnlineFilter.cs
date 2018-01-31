// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
namespace MathNet.Filtering
{
    /// <summary>
    /// An online filter that allows processing samples just in time.
    /// Online Filters are always causal.
    /// </summary>
    public interface IOnlineFilter
    {
        /// <summary>
        /// Process a single sample.
        /// </summary>
        float ProcessSample(float sample);

        /// <summary>
        /// Process a whole set of samples at once.
        /// </summary>
        float[] ProcessSamples(float[] samples);

        /// <summary>
        /// Reset internal state (not coefficients!).
        /// </summary>
        void Reset();
    }
}
