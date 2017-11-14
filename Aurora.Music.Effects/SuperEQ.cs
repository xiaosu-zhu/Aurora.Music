using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
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
            throw new NotImplementedException();
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
                AudioEncodingProperties encodingProps1 = AudioEncodingProperties.CreatePcm(44100, 1, 32);
                encodingProps1.Subtype = MediaEncodingSubtypes.Float;
                AudioEncodingProperties encodingProps2 = AudioEncodingProperties.CreatePcm(48000, 1, 32);
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
