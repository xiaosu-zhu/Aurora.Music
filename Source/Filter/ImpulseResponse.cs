// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
namespace MathNet.Filtering
{
    /// <summary>
    /// Specifies how a filter will respond to an impulse input.
    /// </summary>
    public enum ImpulseResponse
    {
        /// <summary>
        /// Impulse response always has a finite length of time and are stable, but usually have a long delay.
        /// </summary>
        Finite,

        /// <summary>
        /// Impulse response may have an infinite length of time and may be unstable, but usually have only a short delay.
        /// </summary>
        Infinite
    }
}
