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
    public sealed class SuperEQ : IBasicAudioEffect
    {
        public void SetEncodingProperties(AudioEncodingProperties encodingProperties)
        {
            throw new NotImplementedException();
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

                    float inputData;
                    float echoData;

                    // Process audio data
                    int dataInFloatLength = (int)inputBuffer.Length / sizeof(float);

                    for (int i = 0; i < dataInFloatLength; i++)
                    {
                        inputData = inputDataInFloat[i] * (1.0f - this.Mix);
                        echoData = echoBuffer[currentActiveSampleIndex] * this.Mix;
                        outputDataInFloat[i] = inputData + echoData;
                        echoBuffer[currentActiveSampleIndex] = inputDataInFloat[i];
                        currentActiveSampleIndex++;

                        if (currentActiveSampleIndex == echoBuffer.Length)
                        {
                            // Wrap around (after one second of samples)
                            currentActiveSampleIndex = 0;
                        }
                    }

                }
            }
        }

        public void Close(MediaEffectClosedReason reason)
        {
            throw new NotImplementedException();
        }

        public void DiscardQueuedFrames()
        {
            throw new NotImplementedException();
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

        public bool UseInputFrameForOutput => throw new NotImplementedException();

        public void SetProperties(IPropertySet configuration)
        {
            throw new NotImplementedException();
        }
    }
}
