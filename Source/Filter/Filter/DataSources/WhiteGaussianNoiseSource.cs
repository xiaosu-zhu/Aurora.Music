// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using MathNet.Filtering.Channel;
using MathNet.Numerics.Distributions;

namespace MathNet.Filtering.DataSources
{
    /// <summary>
    /// Sample source with independent amplitudes of normal distribution and a flat spectral density.
    /// </summary>
    public class WhiteGaussianNoiseSource : IChannelSource
    {
        readonly IContinuousDistribution _distribution;

        /// <summary>
        /// Create a gaussian noise source with normally distributed amplitudes.
        /// </summary>
        /// <param name="uniformWhiteRandomSource">Uniform white random source.</param>
        /// <param name="mean">mu-parameter of the normal distribution</param>
        /// <param name="standardDeviation">sigma-parameter of the normal distribution</param>
        public WhiteGaussianNoiseSource(Random uniformWhiteRandomSource, float mean, float standardDeviation)
        {
            _distribution = new Normal(mean, standardDeviation, uniformWhiteRandomSource);
        }

        /// <summary>
        /// Create a gaussian noise source with normally distributed amplites.
        /// </summary>
        /// <param name="mean">mu-parameter of the normal distribution</param>
        /// <param name="standardDeviation">sigma-parameter of the normal distribution</param>
        public WhiteGaussianNoiseSource(float mean, float standardDeviation)
        {
            // assuming the default random source is white
            _distribution = new Normal(mean, standardDeviation);
        }

        /// <summary>
        /// Create a gaussian noise source with standard distributed amplitudes.
        /// </summary>
        public WhiteGaussianNoiseSource()
        {
            // assuming the default random source is white
            _distribution = new Normal();
        }

        /// <summary>
        /// Computes and returns the next sample.
        /// </summary>
        public float ReadNextSample()
        {
            return (float)_distribution.Sample();
        }

        /// <summary>
        /// Sample delay of this source in relation to the whole system. Constant Zero.
        /// </summary>
        public int Delay
        {
            get { return 0; }
        }
    }
}
