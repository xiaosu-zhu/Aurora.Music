// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;

namespace Aurora.Music.Effects
{
    public sealed class Threshold : IBasicAudioEffect
    {
        private AudioEncodingProperties currentEncodingProperties;
        private IPropertySet configuration;

        private float thresholdInFloat = 1.0f;
        private float thresholdInFloatR = -1.0f;

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

                    float d;
                    // Process audio data
                    for (int n = 0; n < dataInFloatLength; n++)
                    {
                        d = inputDataInFloat[n];
                        if (d > thresholdInFloat)
                        {
                            inputDataInFloat[n] = thresholdInFloat;
                        }
                        else if (d < thresholdInFloatR)
                        {
                            inputDataInFloat[n] = thresholdInFloatR;
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
            if (configuration.TryGetValue("Threshold", out var o))
            {
                // db -100 ~0
                var hold = (float)o;
                hold /= 20;
                thresholdInFloat = Convert.ToSingle(Math.Pow(10, hold));
                thresholdInFloatR = -thresholdInFloat;
            }
            else
            {
                thresholdInFloat = 1f;
                thresholdInFloatR = -1f;
            }
        }
    }
}
