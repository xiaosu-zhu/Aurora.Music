// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System.Collections.Generic;

using Aurora.Music.Core.Models;

using NAudio.Dsp;

using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;

namespace Aurora.Music.Effects
{
    public sealed class Threshold : IBasicAudioEffect
    {
        private static Threshold current;
        public static Threshold Current
        {
            get
            {
                return current;
            }
        }
        public Threshold()
        {
            current = this;
        }

        private AudioEncodingProperties currentEncodingProperties;
        private IPropertySet configuration;
        private SimpleCompressor compressor = new SimpleCompressor();

        // X : 1
        private float ratio = Settings.Current.CompressorRatio;
        public float Ratio
        {
            get => ratio;
            set
            {
                ratio = value;
                compressor.Ratio = value;
            }
        }
        // - xx dB
        private float makeUpGain = Settings.Current.CompressorMakeUpGain;
        public float MakeUpGain
        {
            get => makeUpGain;
            set
            {
                makeUpGain = value;
                compressor.MakeUpGain = value;
            }
        }
        // ms
        private float attack = Settings.Current.CompressorAttack;
        public float Attack
        {
            get => attack;
            set
            {
                attack = value;
                compressor.Attack = value;
            }
        }
        // ms
        private float release = Settings.Current.CompressorRelease;
        public float Release
        {
            get => release;
            set
            {
                release = value;
                compressor.Release = value;
            }
        }
        // - xx dB
        private float thresholddB = Settings.Current.CompressorThresholddB;
        public float ThresholddB
        {
            get => thresholddB;
            set
            {
                thresholddB = value;
                compressor.Threshold = value;
            }
        }


        public void SetEncodingProperties(AudioEncodingProperties encodingProperties)
        {
            currentEncodingProperties = encodingProperties;
            compressor.SampleRate = currentEncodingProperties.SampleRate;
            compressor.InitRuntime();
            compressor.Ratio = 4f;
            compressor.MakeUpGain = 6f;
        }

        public void ProcessFrame(ProcessAudioFrameContext context)
        {
            unsafe
            {
                var inputFrame = context.InputFrame;

                using (var inputBuffer = inputFrame.LockBuffer(AudioBufferAccessMode.ReadWrite))
                using (var inputReference = inputBuffer.CreateReference())
                {
                    ((IMemoryBufferByteAccess)inputReference).GetBuffer(out byte* inputDataInBytes, out uint inputCapacity);

                    float* inputDataInFloat = (float*)inputDataInBytes;
                    int dataInFloatLength = (int)inputBuffer.Length / sizeof(float);

                    for (int i = 0; i < dataInFloatLength; i += 2)
                    {
                        compressor.Process(ref inputDataInFloat[i], ref inputDataInFloat[i + 1]);
                    }
                }
            }
        }

        public void Close(MediaEffectClosedReason reason)
        {
            switch (reason)
            {
                case MediaEffectClosedReason.Done:
                    break;
                case MediaEffectClosedReason.UnknownError:
                    break;
                case MediaEffectClosedReason.UnsupportedEncodingFormat:
                    break;
                case MediaEffectClosedReason.EffectCurrentlyUnloaded:
                    break;
                default:
                    break;
            }
            compressor.InitRuntime();
        }

        public void DiscardQueuedFrames()
        {
        }

        public IReadOnlyList<AudioEncodingProperties> SupportedEncodingProperties
        {
            get
            {
                var supportedEncodingProperties = new List<AudioEncodingProperties>();

                var encodingProps3 = AudioEncodingProperties.CreatePcm(44100, 2, 32);
                encodingProps3.Subtype = MediaEncodingSubtypes.Float;
                var encodingProps4 = AudioEncodingProperties.CreatePcm(48000, 2, 32);
                encodingProps4.Subtype = MediaEncodingSubtypes.Float;

                var encodingProps5 = AudioEncodingProperties.CreatePcm(96000, 2, 32);
                encodingProps5.Subtype = MediaEncodingSubtypes.Float;
                var encodingProps6 = AudioEncodingProperties.CreatePcm(192000, 2, 32);
                encodingProps6.Subtype = MediaEncodingSubtypes.Float;
                supportedEncodingProperties.Add(encodingProps3);
                supportedEncodingProperties.Add(encodingProps4);
                supportedEncodingProperties.Add(encodingProps5);
                supportedEncodingProperties.Add(encodingProps6);

                return supportedEncodingProperties;
            }
        }

        public bool UseInputFrameForOutput => true;

        public void SetProperties(IPropertySet configuration)
        {
            this.configuration = configuration;
        }
    }
}
