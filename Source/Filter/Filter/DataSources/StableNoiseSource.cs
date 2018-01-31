// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using MathNet.Filtering.Channel;
using MathNet.Numerics.Distributions;

namespace MathNet.Filtering.DataSources
{
    /// <summary>
    /// Sample source with skew alpha stable distributed samples.
    /// </summary>
    public class StableNoiseSource : IChannelSource
    {
        readonly IContinuousDistribution _distribution;

        /// <summary>
        /// Create a skew alpha stable noise source.
        /// </summary>
        /// <param name="uniformWhiteRandomSource">Uniform white random source.</param>
        /// <param name="location">mu-parameter of the stable distribution</param>
        /// <param name="scale">c-parameter of the stable distribution</param>
        /// <param name="exponent">alpha-parameter of the stable distribution</param>
        /// <param name="skewness">beta-parameter of the stable distribution</param>
        public StableNoiseSource(Random uniformWhiteRandomSource, float location, float scale, float exponent, float skewness)
        {
            _distribution = new Stable(exponent, skewness, scale, location, uniformWhiteRandomSource);
        }

        /// <summary>
        /// Create a skew alpha stable noise source.
        /// </summary>
        /// <param name="location">mu-parameter of the stable distribution</param>
        /// <param name="scale">c-parameter of the stable distribution</param>
        /// <param name="exponent">alpha-parameter of the stable distribution</param>
        /// <param name="skewness">beta-parameter of the stable distribution</param>
        public StableNoiseSource(float location, float scale, float exponent, float skewness)
        {
            _distribution = new Stable(exponent, skewness, scale, location);
        }

        /// <summary>
        /// Create a skew alpha stable noise source.
        /// </summary>
        /// <param name="exponent">alpha-parameter of the stable distribution</param>
        /// <param name="skewness">beta-parameter of the stable distribution</param>
        public StableNoiseSource(float exponent, float skewness)
        {
            _distribution = new Stable(exponent, skewness, 1.0, 0.0);
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
