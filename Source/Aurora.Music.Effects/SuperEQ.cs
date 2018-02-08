using MathNet.Filtering.IIR;
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
        private AudioEncodingProperties currentEncodingProperties;
        private OnlineIirFilter[] filter;
        private IPropertySet configuration;

        public void SetEncodingProperties(AudioEncodingProperties encodingProperties)
        {
            currentEncodingProperties = encodingProperties;
            filter = new OnlineIirFilter[]
            {
                new OnlineIirFilter(IirCoefficients.BandPass(currentEncodingProperties.SampleRate, 20, 20000)),
                new OnlineIirFilter(IirCoefficients.BandPass(currentEncodingProperties.SampleRate, 800, 1400)),
                new OnlineIirFilter(IirCoefficients.BandPass(currentEncodingProperties.SampleRate, 1000, 1600)),
                new OnlineIirFilter(IirCoefficients.BandPass(currentEncodingProperties.SampleRate, 1200, 1800)),
                new OnlineIirFilter(IirCoefficients.BandPass(currentEncodingProperties.SampleRate, 1400, 2000)),
                new OnlineIirFilter(IirCoefficients.BandPass(currentEncodingProperties.SampleRate, 1600, 2200)),
                new OnlineIirFilter(IirCoefficients.BandPass(currentEncodingProperties.SampleRate, 1800, 2400)),
                new OnlineIirFilter(IirCoefficients.BandPass(currentEncodingProperties.SampleRate, 2200, 2600))
            };
        }

        public void ProcessFrame(ProcessAudioFrameContext context)
        {
            unsafe
            {
                AudioFrame inputFrame = context.InputFrame;
                AudioFrame outputFrame = context.OutputFrame;

                using (AudioBuffer inputBuffer = inputFrame.LockBuffer(AudioBufferAccessMode.Read),
                                    outputBuffer = outputFrame.LockBuffer(AudioBufferAccessMode.Write))
                using (IMemoryBufferReference inputReference = inputBuffer.CreateReference(),
                                                outputReference = outputBuffer.CreateReference())
                {
                    ((IMemoryBufferByteAccess)inputReference).GetBuffer(out byte* inputDataInBytes, out uint inputCapacity);
                    ((IMemoryBufferByteAccess)outputReference).GetBuffer(out byte* outputDataInBytes, out uint outputCapacity);

                    float* inputDataInFloat = (float*)inputDataInBytes;
                    float* outputDataInFloat = (float*)outputDataInBytes;

                    // Process audio data
                    int dataInFloatLength = (int)inputBuffer.Length / sizeof(float);


                    filter[0].ProcessSamples(ref inputDataInFloat, dataInFloatLength);


                    for (int i = 0; i < dataInFloatLength; i++)
                    {
                        outputDataInFloat[i] = inputDataInFloat[i];
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
            foreach (var item in filter)
            {
                item.Reset();
            }
        }

        public void DiscardQueuedFrames()
        {
            foreach (var item in filter)
            {
                item.Reset();
            }
        }

        public IReadOnlyList<AudioEncodingProperties> SupportedEncodingProperties
        {
            get
            {
                var supportedEncodingProperties = new List<AudioEncodingProperties>();
                AudioEncodingProperties encodingProps1 = AudioEncodingProperties.CreatePcm(44100, 2, 32);
                encodingProps1.Subtype = MediaEncodingSubtypes.Float;
                AudioEncodingProperties encodingProps2 = AudioEncodingProperties.CreatePcm(48000, 2, 32);
                encodingProps2.Subtype = MediaEncodingSubtypes.Float;

                supportedEncodingProperties.Add(encodingProps1);
                supportedEncodingProperties.Add(encodingProps2);

                return supportedEncodingProperties;

            }
        }

        public bool UseInputFrameForOutput => false;

        public void SetProperties(IPropertySet configuration)
        {
            this.configuration = configuration;
        }
    }
}
