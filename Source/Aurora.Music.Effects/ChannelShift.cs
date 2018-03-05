// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;

namespace Aurora.Music.Effects
{
    public sealed class ChannelShift : IBasicAudioEffect
    {
        private static ChannelShift current;
        public static ChannelShift Current
        {
            get => current;
        }

        private AudioEncodingProperties currentEncodingProperties;
        private IPropertySet configuration;
        private float cutL = 1f;
        private float cutR = 1f;

        public void SetEncodingProperties(AudioEncodingProperties encodingProperties)
        {
            currentEncodingProperties = encodingProperties;
        }

        public void ProcessFrame(ProcessAudioFrameContext context)
        {
            unsafe
            {
                AudioFrame inputFrame = context.InputFrame;

                using (AudioBuffer inputBuffer = inputFrame.LockBuffer(AudioBufferAccessMode.ReadWrite))
                using (IMemoryBufferReference inputReference = inputBuffer.CreateReference())
                {
                    ((IMemoryBufferByteAccess)inputReference).GetBuffer(out byte* inputDataInBytes, out uint inputCapacity);

                    float* inputDataInFloat = (float*)inputDataInBytes;
                    int dataInFloatLength = (int)inputBuffer.Length / sizeof(float);
                    // Process audio data
                    for (int n = 0; n < dataInFloatLength; n++)
                    {
                        if (n % 2 == 0)
                        {
                            inputDataInFloat[n] = inputDataInFloat[n] * cutL;
                        }
                        else
                        {
                            inputDataInFloat[n] = inputDataInFloat[n] * cutR;
                        }
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
        }

        public void DiscardQueuedFrames()
        {
        }

        public IReadOnlyList<AudioEncodingProperties> SupportedEncodingProperties
        {
            get
            {
                var supportedEncodingProperties = new List<AudioEncodingProperties>();

                AudioEncodingProperties encodingProps1 = AudioEncodingProperties.CreatePcm(44100, 1, 32);
                encodingProps1.Subtype = MediaEncodingSubtypes.Float;
                AudioEncodingProperties encodingProps2 = AudioEncodingProperties.CreatePcm(48000, 1, 32);
                encodingProps2.Subtype = MediaEncodingSubtypes.Float;

                AudioEncodingProperties encodingProps3 = AudioEncodingProperties.CreatePcm(44100, 2, 32);
                encodingProps3.Subtype = MediaEncodingSubtypes.Float;
                AudioEncodingProperties encodingProps4 = AudioEncodingProperties.CreatePcm(48000, 2, 32);
                encodingProps4.Subtype = MediaEncodingSubtypes.Float;

                AudioEncodingProperties encodingProps5 = AudioEncodingProperties.CreatePcm(96000, 2, 32);
                encodingProps5.Subtype = MediaEncodingSubtypes.Float;
                AudioEncodingProperties encodingProps6 = AudioEncodingProperties.CreatePcm(192000, 2, 32);
                encodingProps6.Subtype = MediaEncodingSubtypes.Float;

                supportedEncodingProperties.Add(encodingProps1);
                supportedEncodingProperties.Add(encodingProps2);
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
            if (configuration.TryGetValue("Shift", out var o))
            {
                // shift -1~1
                // -1: left = 1, right = 0;
                // 1: left = 0, right = 1;
                var shift = (float)o;
                if (shift > 1 || shift < -1)
                {
                    shift = 0f;
                }
                cutL = Convert.ToSingle(1 - Math.Max(0, shift));
                cutR = Convert.ToSingle(1 + Math.Min(0, shift));
            }
            else
            {
                cutL = 1f;
                cutR = 1f;
            }
        }

        public ChannelShift()
        {
            current = this;
        }

        public void ChangeShift(float shift)
        {
            if (shift > 1 || shift < -1)
            {
                shift = 0f;
            }
            cutL = Convert.ToSingle(1 - Math.Max(0, shift));
            cutR = Convert.ToSingle(1 + Math.Min(0, shift));
        }
    }
}
