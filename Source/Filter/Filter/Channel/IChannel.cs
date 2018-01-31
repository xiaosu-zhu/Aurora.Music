// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
namespace MathNet.Filtering.Channel
{
    /// <summary>
    /// Sample Provider
    /// </summary>
    public interface IChannelSource
    {
        /// <summary>
        /// Computes and returns the next sample.
        /// </summary>
        float ReadNextSample();

        /// <summary>
        /// Sample delay of this source in relation to the whole system.
        /// </summary>
        int Delay { get; }
    }
}
