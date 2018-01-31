// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;

namespace MathNet.Filtering.Windowing
{
    /// <summary>
    /// A windowing/apodization function.
    /// </summary>
    public interface IWindow
    {
        /// <summary>
        /// Window width, number of samples. Typically an integer power of 2.
        /// </summary>
        int Width { get; set; }

        /// <summary>
        /// Window coefficients
        /// </summary>
        /// <param name="sampleIndex">window sample index, between 0 and Width-1 (inclusive).</param>
        float this[int sampleIndex] { get; }

        /// <summary>
        /// Copy the window coefficients to a float array.
        /// </summary>
        float[] CopyToArray();

        /// <summary>
        /// Compute the window for the current configuration.
        /// Skipped if the window has already been computed.
        /// </summary>
        void Precompute();
    }

    /// <summary>
    /// A windowing/apodization function.
    /// </summary>
    public abstract class Window :
        IWindow
    {
        int _width = 1; // N
        float[] _coefficients;

        /// <summary>
        /// Default constructor
        /// </summary>
        protected Window()
        {
        }

        /// <summary>
        /// Windowing function generator implementation.
        /// </summary>
        /// <param name="width">window size, guaranteed to be greater than or equal to 2.</param>
        /// <returns>Window coefficients, array with length of 'width'</returns>
        protected abstract float[] ComputeWindowCore(int width);

        /// <summary>
        /// Reset the coefficients, triggering Precompute the next time a coefficient is requested.
        /// </summary>
        protected void Reset()
        {
            _coefficients = null;
        }

        /// <summary>
        /// Compute the window for the current configuration.
        /// Skipped if the window has already been computed.
        /// </summary>
        public void Precompute()
        {
            if (null != _coefficients)
            {
                // coefficients already available, skip
                return;
            }

            if (_width == 1)
            {
                // trivial window
                _coefficients = new float[1];
                _coefficients[0] = 1.0f;
                return;
            }

            _coefficients = ComputeWindowCore(_width);

            if (null == _coefficients || _width != _coefficients.Length)
            {
                throw new Exception("InvalidWindowFunctionException");
            }
        }

        /// <summary>
        /// Copy the window coefficients to a float array.
        /// </summary>
        public float[] CopyToArray()
        {
            Precompute();
            float[] data = new float[_width];
            _coefficients.CopyTo(data, 0);
            return data;
        }

        /// <summary>
        /// Window width, number of samples. Typically an integer power of 2, must be greater than 0.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public int Width
        {
            get { return _width; }
            set
            {
                if (0 >= value)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                _width = value;
                Reset();
            }
        }

        /// <summary>
        /// Window coefficients
        /// </summary>
        /// <param name="sampleIndex">window sample index, between 0 and Width-1 (inclusive).</param>
        public float this[int sampleIndex]
        {
            get
            {
                if (0 > sampleIndex || _width <= sampleIndex)
                {
                    throw new ArgumentOutOfRangeException("sampleIndex");
                }

                if (null == _coefficients)
                {
                    Precompute();
                }

                return _coefficients[sampleIndex];
            }
        }
    }
}
