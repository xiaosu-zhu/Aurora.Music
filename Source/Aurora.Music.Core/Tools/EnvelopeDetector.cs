// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.

// based on EnvelopeDetector.cpp v1.10 � 2006, ChunkWare Music Software, OPEN-SOURCE
using NAudio.Utils;
using System;

namespace NAudio.Dsp
{
    public sealed class EnvelopeDetector
    {
        private double sampleRate;
        private double ms;
        private double coeff;

        public EnvelopeDetector() : this(1.0, 44100.0)
        {
        }

        public EnvelopeDetector(double ms, double sampleRate)
        {
            System.Diagnostics.Debug.Assert(sampleRate > 0.0);
            System.Diagnostics.Debug.Assert(ms > 0.0);
            this.sampleRate = sampleRate;
            this.ms = ms;
            SetCoef();
        }

        public double TimeConstant
        {
            get
            {
                return ms;
            }
            set
            {
                System.Diagnostics.Debug.Assert(value > 0.0);
                this.ms = value;
                SetCoef();
            }
        }

        public double SampleRate
        {
            get
            {
                return sampleRate;
            }
            set
            {
                System.Diagnostics.Debug.Assert(value > 0.0);
                this.sampleRate = value;
                SetCoef();
            }
        }

        public void Run(double inValue, ref double state)
        {
            state = inValue + coeff * (state - inValue);
        }

        private void SetCoef()
        {
            coeff = Math.Exp(-1.0 / (0.001 * ms * sampleRate));
        }
    }

    public class AttRelEnvelope
    {
        // DC offset to prevent denormal
        protected const double DC_OFFSET = 1.0E-25;

        private readonly EnvelopeDetector attack;
        private readonly EnvelopeDetector release;

        public AttRelEnvelope(double attackMilliseconds, double releaseMilliseconds, double sampleRate)
        {
            attack = new EnvelopeDetector(attackMilliseconds, sampleRate);
            release = new EnvelopeDetector(releaseMilliseconds, sampleRate);
        }

        public double Attack
        {
            get { return attack.TimeConstant; }
            set { attack.TimeConstant = value; }
        }

        public double Release
        {
            get { return release.TimeConstant; }
            set { release.TimeConstant = value; }
        }

        public double SampleRate
        {
            get { return attack.SampleRate; }
            set { attack.SampleRate = release.SampleRate = value; }
        }

        public void Run(double inValue, ref double state)
        {
            // assumes that:
            // positive delta = attack
            // negative delta = release
            // good for linear & log values
            if (inValue > state)
                attack.Run(inValue, ref state);   // attack
            else
                release.Run(inValue, ref state);  // release
        }
    }

    // based on SimpleGate v1.10 � 2006, ChunkWare Music Software, OPEN-SOURCE
    class SimpleGate : AttRelEnvelope
    {
        // transfer function
        private double threshdB;    // threshold (dB)
        private double thresh;      // threshold (linear)

        // runtime variables
        private double env;     // over-threshold envelope (linear)

        public SimpleGate()
            : base(10.0, 10.0, 44100.0)
        {
            threshdB = 0.0;
            thresh = 1.0;
            env = DC_OFFSET;
        }

        public void Process(ref double in1, ref double in2)
        {
            // in/out pointers are assummed to reference stereo data

            // sidechain

            // rectify input
            double rect1 = Math.Abs(in1);   // n.b. was fabs
            double rect2 = Math.Abs(in2); // n.b. was fabs

            // if desired, one could use another EnvelopeDetector to smooth
            // the rectified signal.

            double key = Math.Max(rect1, rect2);    // link channels with greater of 2

            // threshold
            double over = (key > thresh) ? 1.0 : 0.0;   // key over threshold ( 0.0 or 1.0 )

            // attack/release
            over += DC_OFFSET;              // add DC offset to avoid denormal

            Run(over, ref env); // run attack/release

            over = env - DC_OFFSET;     // subtract DC offset

            // Regarding the DC offset: In this case, since the offset is added before 
            // the attack/release processes, the envelope will never fall below the offset,
            // thereby avoiding denormals. However, to prevent the offset from causing
            // constant gain reduction, we must subtract it from the envelope, yielding
            // a minimum value of 0dB.

            // output gain
            in1 *= over;    // apply gain reduction to input
            in2 *= over;
        }

        public double Threshold
        {
            get
            {
                return threshdB;
            }
            set
            {
                threshdB = value;
                thresh = Decibels.DecibelsToLinear(value);
            }
        }
    }

    // based on SimpleComp v1.10 � 2006, ChunkWare Music Software, OPEN-SOURCE
    class SimpleCompressor : AttRelEnvelope
    {
        // transfer function

        // runtime variables
        private double envdB;           // over-threshold envelope (dB)

        public SimpleCompressor(double attackTime, double releaseTime, double sampleRate)
            : base(attackTime, releaseTime, sampleRate)
        {
            this.Threshold = 0.0;
            this.Ratio = 1.0;
            this.MakeUpGain = 0.0;
            this.envdB = DC_OFFSET;
        }

        public SimpleCompressor()
            : base(10.0, 10.0, 44100.0)
        {
            this.Threshold = 0.0;
            this.Ratio = 1.0;
            this.MakeUpGain = 0.0;
            this.envdB = DC_OFFSET;
        }

        public double MakeUpGain { get; set; }

        public double Threshold { get; set; }

        public double Ratio { get; set; }

        // call before runtime (in resume())
        public void InitRuntime()
        {
            this.envdB = DC_OFFSET;
        }

        // // compressor runtime process
        public void Process(ref double in1, ref double in2)
        {
            // sidechain

            // rectify input
            double rect1 = Math.Abs(in1);   // n.b. was fabs
            double rect2 = Math.Abs(in2); // n.b. was fabs

            // if desired, one could use another EnvelopeDetector to smooth
            // the rectified signal.

            double link = Math.Max(rect1, rect2);   // link channels with greater of 2

            link += DC_OFFSET;                  // add DC offset to avoid log( 0 )
            double keydB = Decibels.LinearToDecibels(link);     // convert linear -> dB

            // threshold
            double overdB = keydB - Threshold;  // delta over threshold
            if (overdB < 0.0)
                overdB = 0.0;

            // attack/release

            overdB += DC_OFFSET;                    // add DC offset to avoid denormal

            Run(overdB, ref envdB); // run attack/release envelope

            overdB = envdB - DC_OFFSET;         // subtract DC offset

            // Regarding the DC offset: In this case, since the offset is added before 
            // the attack/release processes, the envelope will never fall below the offset,
            // thereby avoiding denormals. However, to prevent the offset from causing
            // constant gain reduction, we must subtract it from the envelope, yielding
            // a minimum value of 0dB.

            // transfer function
            double gr = overdB * (Ratio - 1.0); // gain reduction (dB)
            gr = Decibels.DecibelsToLinear(gr) * Decibels.DecibelsToLinear(MakeUpGain); // convert dB -> linear

            // output gain
            in1 *= gr;  // apply gain reduction to input
            in2 *= gr;
        }
    }
}


namespace NAudio.Utils
{
    /// <summary>
    /// A util class for conversions
    /// </summary>
    public class Decibels
    {
        // 20 / ln( 10 )
        private const double LOG_2_DB = 8.6858896380650365530225783783321;

        // ln( 10 ) / 20
        private const double DB_2_LOG = 0.11512925464970228420089957273422;

        /// <summary>
        /// linear to dB conversion
        /// </summary>
        /// <param name="lin">linear value</param>
        /// <returns>decibel value</returns>
        public static double LinearToDecibels(double lin)
        {
            return Math.Log(lin) * LOG_2_DB;
        }

        /// <summary>
        /// dB to linear conversion
        /// </summary>
        /// <param name="dB">decibel value</param>
        /// <returns>linear value</returns>
        public static double DecibelsToLinear(double dB)
        {
            return Math.Exp(dB * DB_2_LOG);
        }

    }
}