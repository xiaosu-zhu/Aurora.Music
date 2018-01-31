// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MathNet.Filtering.Channel;
using MathNet.Numerics.Distributions;

namespace MathNet.Filtering.DataSources
{
    /// <summary>
    /// Adapter for Iridium continuous (random) generators to Neodym channel sources.
    /// </summary>
    public class GeneratorSource : IChannelSource
    {
        readonly IContinuousDistribution _distribution;

        /// <summary>
        /// Create a sample source from a continuous generator.
        /// </summary>
        public
            GeneratorSource(IContinuousDistribution distribution)
        {
            _distribution = distribution;
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
