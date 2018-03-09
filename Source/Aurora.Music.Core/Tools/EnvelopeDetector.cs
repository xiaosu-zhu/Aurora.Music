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
        private float sampleRate;
        private float ms;
        private float coeff;

        public EnvelopeDetector() : this(1.0f, 44100.0f)
        {
        }

        public EnvelopeDetector(float ms, float sampleRate)
        {
            System.Diagnostics.Debug.Assert(sampleRate > 0.0f);
            System.Diagnostics.Debug.Assert(ms > 0.0f);
            this.sampleRate = sampleRate;
            this.ms = ms;
            SetCoef();
        }

        public float TimeConstant
        {
            get
            {
                return ms;
            }
            set
            {
                System.Diagnostics.Debug.Assert(value > 0.0f);
                this.ms = value;
                SetCoef();
            }
        }

        public float SampleRate
        {
            get
            {
                return sampleRate;
            }
            set
            {
                System.Diagnostics.Debug.Assert(value > 0.0f);
                this.sampleRate = value;
                SetCoef();
            }
        }

        public void Run(float inValue, ref float state)
        {
            state = inValue + coeff * (state - inValue);
        }

        private void SetCoef()
        {
            coeff = Convert.ToSingle(Math.Exp(-1.0f / (0.001f * ms * sampleRate)));
        }
    }

    public class AttRelEnvelope
    {
        // DC offset to prevent denormal
        protected const float DC_OFFSET = 1.0E-25F;

        private readonly EnvelopeDetector attack;
        private readonly EnvelopeDetector release;

        public AttRelEnvelope(float attackMilliseconds, float releaseMilliseconds, float sampleRate)
        {
            attack = new EnvelopeDetector(attackMilliseconds, sampleRate);
            release = new EnvelopeDetector(releaseMilliseconds, sampleRate);
        }

        public float Attack
        {
            get { return attack.TimeConstant; }
            set { attack.TimeConstant = value; }
        }

        public float Release
        {
            get { return release.TimeConstant; }
            set { release.TimeConstant = value; }
        }

        public float SampleRate
        {
            get { return attack.SampleRate; }
            set { attack.SampleRate = release.SampleRate = value; }
        }

        public void Run(float inValue, ref float state)
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
    public class SimpleGate : AttRelEnvelope
    {
        // transfer function
        private float threshdB;    // threshold (dB)
        private float thresh;      // threshold (linear)

        // runtime variables
        private float env;     // over-threshold envelope (linear)

        public SimpleGate()
            : base(10.0f, 10.0f, 44100.0f)
        {
            threshdB = 0.0f;
            thresh = 1.0f;
            env = DC_OFFSET;
        }

        public void Process(ref float in1, ref float in2)
        {
            // in/out pointers are assummed to reference stereo data

            // sidechain

            // rectify input
            float rect1 = Math.Abs(in1);   // n.b. was fabs
            float rect2 = Math.Abs(in2); // n.b. was fabs

            // if desired, one could use another EnvelopeDetector to smooth
            // the rectified signal.

            float key = Math.Max(rect1, rect2);    // link channels with greater of 2

            // threshold
            float over = (key > thresh) ? 1.0f : 0.0f;   // key over threshold ( 0.0 or 1.0 )

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

        public float Threshold
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
    public class SimpleCompressor : AttRelEnvelope
    {
        // transfer function

        // runtime variables
        private float envdB;           // over-threshold envelope (dB)

        public SimpleCompressor(float attackTime, float releaseTime, float sampleRate)
            : base(attackTime, releaseTime, sampleRate)
        {
            this.Threshold = 0.0f;
            this.Ratio = 1.0f;
            this.MakeUpGain = 0.0f;
            this.envdB = DC_OFFSET;
        }

        public SimpleCompressor()
            : base(10.0f, 10.0f, 44100.0f)
        {
            this.Threshold = 0.0f;
            this.Ratio = 1.0f;
            this.MakeUpGain = 0.0f;
            this.envdB = DC_OFFSET;
        }

        public float MakeUpGain { get; set; }

        public float Threshold { get; set; }

        public float Ratio { get; set; }

        // call before runtime (in resume())
        public void InitRuntime()
        {
            this.envdB = DC_OFFSET;
        }

        // // compressor runtime process
        public void Process(ref float in1, ref float in2)
        {
            // sidechain

            // rectify input
            float rect1 = Math.Abs(in1);   // n.b. was fabs
            float rect2 = Math.Abs(in2); // n.b. was fabs

            // if desired, one could use another EnvelopeDetector to smooth
            // the rectified signal.

            float link = Math.Max(rect1, rect2);   // link channels with greater of 2

            link += DC_OFFSET;                  // add DC offset to avoid log( 0 )
            float keydB = Decibels.LinearToDecibels(link);     // convert linear -> dB

            // threshold
            float overdB = keydB - Threshold;  // delta over threshold
            if (overdB < 0.0f)
                overdB = 0.0f;

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
            float gr = overdB * (Ratio - 1.0f); // gain reduction (dB)
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
        private const float LOG_2_DB = 8.6858896380650365530225783783321f;

        // ln( 10 ) / 20
        private const float DB_2_LOG = 0.11512925464970228420089957273422f;

        /// <summary>
        /// linear to dB conversion
        /// </summary>
        /// <param name="lin">linear value</param>
        /// <returns>decibel value</returns>
        public static float LinearToDecibels(float lin)
        {
            return Convert.ToSingle(Math.Log(lin) * LOG_2_DB);
        }

        /// <summary>
        /// dB to linear conversion
        /// </summary>
        /// <param name="dB">decibel value</param>
        /// <returns>linear value</returns>
        public static float DecibelsToLinear(float dB)
        {
            return Convert.ToSingle(Math.Exp(dB * DB_2_LOG));
        }

    }
}