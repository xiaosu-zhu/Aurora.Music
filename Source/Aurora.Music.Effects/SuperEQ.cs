using NAudio.Dsp;
using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;

namespace Aurora.Music.Effects
{
    public sealed class SuperEQ : IBasicAudioEffect
    {
        private static SuperEQ current;
        public static SuperEQ Current
        {
            get
            {
                return current;
            }
        }

        public event TypedEventHandler<object, IReadOnlyList<float>> FFTCompleted;

        public SuperEQ()
        {
            current = this;
            fftData.Initialize();

            for (int i = 0; i < 1024; i++)
            {
                hannWindow[i] = (float)FastFourierTransform.HannWindow(i, 1024);
            }
        }

        public void UpdateEqualizerBand(IReadOnlyList<float> equalizerBand)
        {
            if (bandCount != equalizerBand.Count)
            {
                throw new ArgumentException("Bands Count mismatch");
            }
            for (int i = 0; i < bandCount; i++)
            {
                bands[i].Gain = equalizerBand[i];
            }
            CreateFilters();
        }

        private AudioEncodingProperties currentEncodingProperties;
        private EqualizerBand[] bands;
        private BiQuadFilter[,] filters;
        private int channels;
        private int bandCount;
        private IPropertySet configuration;

        private readonly Complex[] fftData = new Complex[1024];
        private int currentfftData = 0;
        private readonly float[] hannWindow = new float[1024];
        private readonly float[] fftMagnitude = new float[512];

        public void SetEncodingProperties(AudioEncodingProperties encodingProperties)
        {
            currentEncodingProperties = encodingProperties;

            bands = ReadConfiguration();
            if (channels != (int)currentEncodingProperties.ChannelCount || bandCount != bands.Length)
            {
                channels = (int)currentEncodingProperties.ChannelCount;
                bandCount = bands.Length;

                filters = new BiQuadFilter[channels, bandCount];
            }
            CreateFilters();
        }

        private EqualizerBand[] ReadConfiguration()
        {
            if (bands != null)
            {
                return bands;
            }
            if (configuration != null && configuration.TryGetValue("EqualizerBand", out object t) && t is EqualizerBand[] e)
            {
                return e;
            }
            return new EqualizerBand[]
            {
                new EqualizerBand {Bandwidth = 0.8f, Frequency = 30, Gain = 0},
                new EqualizerBand {Bandwidth = 0.8f, Frequency = 75, Gain = 0},
                new EqualizerBand {Bandwidth = 0.8f, Frequency = 150, Gain = 0},
                new EqualizerBand {Bandwidth = 0.8f, Frequency = 30, Gain = 0},
                new EqualizerBand {Bandwidth = 0.8f, Frequency = 600, Gain = 0},
                new EqualizerBand {Bandwidth = 0.8f, Frequency = 1250, Gain = 0},
                new EqualizerBand {Bandwidth = 0.8f, Frequency = 2500, Gain = 0},
                new EqualizerBand {Bandwidth = 0.8f, Frequency = 5000, Gain = 0},
                new EqualizerBand {Bandwidth = 0.8f, Frequency = 10000, Gain = 0},
                new EqualizerBand {Bandwidth = 0.8f, Frequency = 20000, Gain = 0},
            };
        }

        private void CreateFilters()
        {
            for (int bandIndex = 0; bandIndex < bandCount; bandIndex++)
            {
                var band = bands[bandIndex];
                for (int n = 0; n < channels; n++)
                {
                    if (filters[n, bandIndex] == null)
                        filters[n, bandIndex] = BiQuadFilter.PeakingEQ(currentEncodingProperties.SampleRate, band.Frequency, band.Bandwidth, band.Gain);
                    else
                        filters[n, bandIndex].SetPeakingEq(currentEncodingProperties.SampleRate, band.Frequency, band.Bandwidth, band.Gain);
                }
            }
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
                        int ch = n % channels;

                        if (ch == 0)
                        {
                            // stereo to mono
                            float a = inputDataInFloat[n];
                            for (int j = 1; j < channels; j++)
                            {
                                a += inputDataInFloat[n + j];
                            }
                            a /= channels;

                            // apply a HannWindow
                            fftData[currentfftData].X = a * hannWindow[currentfftData];
                            fftData[currentfftData].Y = 0;

                            currentfftData++;
                            // when it hit 1024, perform a fft
                            if (currentfftData == 1024)
                            {
                                currentfftData = 0;
                                FastFourierTransform.FFT(true, 10, fftData);
                                for (int i = 0; i < 512; i++)
                                {
                                    fftMagnitude[i] = MathF.Sqrt(fftData[i].X * fftData[i].X + fftData[i].Y * fftData[i].Y);
                                }
                                FFTCompleted?.Invoke(this, fftMagnitude);
                            }
                        }

                        // cascaded filter to perform eq
                        for (int band = 0; band < bandCount; band++)
                        {
                            inputDataInFloat[n] = filters[ch, band].Transform(inputDataInFloat[n]);
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

            for (int i = 0; i < 1024; i++)
            {
                fftData[i] = new Complex();
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

                supportedEncodingProperties.Add(encodingProps1);
                supportedEncodingProperties.Add(encodingProps2);
                supportedEncodingProperties.Add(encodingProps3);
                supportedEncodingProperties.Add(encodingProps4);

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
