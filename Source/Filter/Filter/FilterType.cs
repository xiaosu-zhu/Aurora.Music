// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
namespace MathNet.Filtering
{
    /// <summary>
    /// Frequency Filter Type
    /// </summary>
    public enum FilterType
    {
        /// <summary>LowPass, lets only low frequencies pass.</summary>
        LowPass,

        /// <summary>HighPass, lets only high frequencies pass.</summary>
        HighPass,

        /// <summary>BandPass, lets only frequencies pass that are inside of a band.</summary>
        BandPass,

        /// <summary>BandStop, lets only frequencies pass that are outside of a band.</summary>
        BandStop,

        /// <summary>Other behavior.</summary>
        Other
    }
}
