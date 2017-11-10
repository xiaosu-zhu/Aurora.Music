using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurora.Music.Core.Models;
using Windows.Media.Audio;
using Windows.Storage;
using Aurora.Shared.Extensions;
using Windows.Devices.Enumeration;

namespace Aurora.Music.Core.Player
{
    public class AudioGraphPlayer : IPlayer, IDisposable
    {
        private static object lockable = new object();


        public event EventHandler<PositionUpdatedArgs> PositionUpdated;
        public event EventHandler<StatusChangedArgs> StatusChanged;

        #region IDisposable
        /*******************IDisposable**************************/
        AudioFileInputNode      fileInputNode;
        AudioGraph              audioGraph;
        AudioDeviceOutputNode   deviceOutputNode;
        AudioSubmixNode         effectsNode;
        /*******************IDisposable**************************/
        #endregion

        private ReverbEffectDefinition reverbEffect;
        private LimiterEffectDefinition limiterEffect;
        private EqualizerEffectDefinition eqEffect;

        private AudioGraphSettings audioSettings;
        private TimeSpan? currentPosition;

        public async void Init()
        {
            var settings = Settings.Load();

            audioSettings = new AudioGraphSettings(Windows.Media.Render.AudioRenderCategory.Media);

            ChangeAudioEndPoint(settings.OutputDeviceID);

            CreateAudioGraphResult result = await AudioGraph.CreateAsync(audioSettings);
            if (result.Status != AudioGraphCreationStatus.Success)
            {
                throw new InvalidOperationException("Can't create AudioGraph");
            }

            audioGraph = result.Graph;
            ChangeVolume(settings.PlayerVolume);
        }

        private async Task CreateFileInputNodeAsync(IStorageFile file)
        {
            if (audioGraph == null)
                return;

            // File can be null if cancel is hit in the file picker
            if (file == null)
            {
                return;
            }
            CreateAudioFileInputNodeResult result = await audioGraph.CreateFileInputNodeAsync(file);

            if (result.Status != AudioFileNodeCreationStatus.Success)
            {
                throw new InvalidOperationException("Can't load file");
            }

            fileInputNode = result.FileInputNode;
        }

        private void InitEffects()
        {
            var settings = Settings.Load();
            reverbEffect = new ReverbEffectDefinition(audioGraph)
            {

            };
            limiterEffect = new LimiterEffectDefinition(audioGraph)
            {

            };
            eqEffect = new EqualizerEffectDefinition(audioGraph)
            {

            };
            effectsNode = audioGraph.CreateSubmixNode();
        }

        private void ApplyEffects(Effects e)
        {

            if (effectsNode == null)
                return;

            effectsNode.RemoveOutgoingConnection(deviceOutputNode);
            fileInputNode.RemoveOutgoingConnection(effectsNode);
            effectsNode.EffectDefinitions.Clear();

            if (e == Effects.Equalizer)
            {
                effectsNode.EffectDefinitions.Add(eqEffect);
            }
            if (e == Effects.Limiter)
            {
                effectsNode.EffectDefinitions.Add(limiterEffect);
            }
            if (e == Effects.Reverb)
            {
                effectsNode.EffectDefinitions.Add(reverbEffect);
            }

            fileInputNode.AddOutgoingConnection(effectsNode);
            effectsNode.AddOutgoingConnection(deviceOutputNode);
        }

        private void ShutdownEffects()
        {
            effectsNode.RemoveOutgoingConnection(deviceOutputNode);
            fileInputNode.RemoveOutgoingConnection(effectsNode);
            fileInputNode.AddOutgoingConnection(deviceOutputNode);
        }

        private async Task CreateDeviceOutputNodeAsync()
        {
            // Create a device output node
            CreateAudioDeviceOutputNodeResult result = await audioGraph.CreateDeviceOutputNodeAsync();

            if (result.Status != AudioDeviceNodeCreationStatus.Success)
            {
                // Cannot create device output node
                throw new InvalidOperationException("Can't init output device");
            }

            deviceOutputNode = result.DeviceOutputNode;
        }

        public async void ChangeAudioEndPoint(string outputDeviceID)
        {
            if (outputDeviceID.IsNullorEmpty() || outputDeviceID == audioGraph.PrimaryRenderDevice?.Id)
            {
                return;
            }
            var outputDevice = await DeviceInformation.CreateFromIdAsync(outputDeviceID);
            audioSettings.PrimaryRenderDevice = outputDevice;
        }

        public void ChangeVolume(double vol)
        {
            // TODO: gain(dB) to volume?
            deviceOutputNode.OutgoingGain = vol;
        }

        public void Loop(bool? isOn)
        {
            if (fileInputNode != null)
            {
                fileInputNode.LoopCount = int.MaxValue;
            }
        }

        public async Task NewPlayList(IList<Song> songs, int startIndex = 0)
        {
            throw new NotImplementedException();
        }

        public void Next()
        {
            throw new NotImplementedException();
        }

        public void Pause()
        {
            currentPosition = fileInputNode?.Position;
            fileInputNode?.Stop();
        }

        public void Play()
        {
            lock (lockable)
            {
                if (fileInputNode != null)
                {
                    fileInputNode.StartTime = currentPosition;
                    fileInputNode.Start();
                }
                else
                {
                    PrepareToPlay();
                    fileInputNode.Start();
                }
            }
        }

        private void PrepareToPlay()
        {
            throw new NotImplementedException();
            PlugInGraph();
        }

        private void PlugInGraph()
        {
            throw new NotImplementedException();
        }

        public void Previous()
        {
            throw new NotImplementedException();
        }

        public void Seek(TimeSpan position)
        {
            fileInputNode?.Seek(position);
        }

        public void Shuffle(bool? isOn)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            UnplugGraph();
            fileInputNode.Dispose();
            fileInputNode = null;
        }

        private void UnplugGraph()
        {
            throw new NotImplementedException();
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~AudioGraphPlayer() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

    [Flags]
    public enum Effects
    {
        None = 0, Reverb = 2, Limiter = 4, Equalizer = 8, All = Reverb | Limiter | Equalizer
    }
}
