// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core.Models;
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
        private float cutL = 1f;
        private float cutR = 1f;
        private bool currentIsMono;

        public void SetEncodingProperties(AudioEncodingProperties encodingProperties)
        {
            currentEncodingProperties = encodingProperties;
            if (currentEncodingProperties.ChannelCount != 2)
            {
                currentIsMono = true;
            }
            else
            {
                currentIsMono = false;
            }
        }

        void StereoWidth(ref float left, ref float right, float width)
        {
            // calc coefs
            var tmp = 1 / Math.Max(1 + width, 2);
            var coef_M = 1 * tmp;
            var coef_S = width * tmp;

            // then do this per sample
            var m = (left + right) * coef_M;
            var s = (right - left) * coef_S;

            left = m - s;
            right = m + s;
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

                    // only stereo can use this
                    if (!currentIsMono)
                    {
                        for (int n = 0; n < dataInFloatLength; n += 2)
                        {
                            var l = inputDataInFloat[n] * cutL;
                            var r = inputDataInFloat[n + 1] * cutR;

                            if (StereoToMono)
                            {
                                inputDataInFloat[n] = l + r / (cutL + cutR);
                                inputDataInFloat[n + 1] = inputDataInFloat[n];
                            }
                            else
                            {
                                inputDataInFloat[n] = l;
                                inputDataInFloat[n + 1] = r;
                            }
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

                AudioEncodingProperties encodingProps3 = AudioEncodingProperties.CreatePcm(44100, 2, 32);
                encodingProps3.Subtype = MediaEncodingSubtypes.Float;
                AudioEncodingProperties encodingProps4 = AudioEncodingProperties.CreatePcm(48000, 2, 32);
                encodingProps4.Subtype = MediaEncodingSubtypes.Float;

                AudioEncodingProperties encodingProps5 = AudioEncodingProperties.CreatePcm(96000, 2, 32);
                encodingProps5.Subtype = MediaEncodingSubtypes.Float;
                AudioEncodingProperties encodingProps6 = AudioEncodingProperties.CreatePcm(192000, 2, 32);
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
            // shift -1~1
            // -1: left = 1, right = 0;
            // 1: left = 0, right = 1;
            var shift = Settings.Current.ChannelShift;
            if (shift > 1 || shift < -1)
            {
                shift = 0f;
            }
            cutL = Convert.ToSingle(1 - Math.Max(0, shift));
            cutR = Convert.ToSingle(1 + Math.Min(0, shift));

            StereoToMono = Settings.Current.StereoToMono;
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

        public bool StereoToMono
        {
            get;
            set;
        } = false;
    }
}
