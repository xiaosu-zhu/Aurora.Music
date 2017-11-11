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
using Windows.System.Threading;
using Aurora.Shared.Helpers;

namespace Aurora.Music.Core.Player
{
    public class AudioGraphPlayer : IPlayer, IDisposable
    {
        private static object lockable = new object();



        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                    audioGraph.Stop();
                    audioGraph.ResetAllNodes();

                    fileInputNode?.Dispose();
                    previousFileNode?.Dispose();
                    nextFileNode?.Dispose();
                    deviceOutputNode?.Dispose();
                    reverbNode?.Dispose();
                    eqNode?.Dispose();
                    limiterNode?.Dispose();

                    audioGraph?.Dispose();
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。

                // TODO: 将大型字段设置为 null。
                currentPlayList.Clear();
                files.Clear();
                currentPlayList = null;
                files = null;

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        ~AudioGraphPlayer()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(false);
        }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            GC.SuppressFinalize(this);
        }
        #endregion

        public event EventHandler<PositionUpdatedArgs> PositionUpdated;
        public event EventHandler<StatusChangedArgs> StatusChanged;

        #region IDisposable
        /*******************IDisposable**************************/

        // use three fileInputNode to cache
        AudioFileInputNode fileInputNode;
        AudioFileInputNode previousFileNode;
        AudioFileInputNode nextFileNode;


        AudioGraph audioGraph;
        AudioDeviceOutputNode deviceOutputNode;
        AudioSubmixNode reverbNode;
        AudioSubmixNode eqNode;
        AudioSubmixNode limiterNode;
        /*******************IDisposable**************************/
        #endregion


        private int currentIndex;
        private List<StorageFile> files;
        private List<Song> currentPlayList;

        private ThreadPoolTimer prepareNextTimer;

        private bool? isPlaying;
        public bool? IsPlaying
        {
            get
            {
                return isPlaying;
            }
        }

        private ReverbEffectDefinition reverbEffect;
        private LimiterEffectDefinition limiterEffect;
        private EqualizerEffectDefinition eqEffect;

        private AudioGraphSettings audioSettings;
        private TimeSpan? currentPosition;
        private bool isLoop;
        private bool isShuffle;
        private bool prepared;
        private ThreadPoolTimer updateTimer;
        private bool graphBuilt;

        public async Task Init()
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
            audioGraph.UnrecoverableErrorOccurred += AudioGraph_UnrecoverableErrorOccurred;
        }

        private void AudioGraph_UnrecoverableErrorOccurred(AudioGraph sender, AudioGraphUnrecoverableErrorOccurredEventArgs args)
        {
            throw new NotImplementedException();
        }

        public AudioGraphPlayer()
        {
            currentPlayList = new List<Song>();
            files = new List<StorageFile>();
            AsyncHelper.RunSync(async () => await Init());
        }

        private async Task<AudioFileInputNode> CreateFileInputNodeAsync(IStorageFile file)
        {
            if (audioGraph == null)
                return null;

            // File can be null if cancel is hit in the file picker
            if (file == null)
            {
                return null;
            }
            CreateAudioFileInputNodeResult result = await audioGraph.CreateFileInputNodeAsync(file);

            if (result.Status != AudioFileNodeCreationStatus.Success)
            {
                throw new InvalidOperationException("Can't load file");
            }

            //prepareNextTimer?.Cancel();
            //prepareNextTimer = null;
            //prepareNextTimer = ThreadPoolTimer.CreateTimer(FileAlmostComplete, result.FileInputNode.Duration - TimeSpan.FromSeconds(3));

            return result.FileInputNode;
        }

        private void FileAlmostComplete(ThreadPoolTimer timer)
        {
            if (currentIndex < 0 || currentIndex >= files.Count - 1 || nextFileNode == null)
            {
                return;
            }

            nextFileNode.StartTime = TimeSpan.Zero;
            // TODO: what's this?
            // nextFileNode.ConsumeInput
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

            reverbNode = audioGraph.CreateSubmixNode();
        }

        private void ApplyEffects(Effects e)
        {

            if (reverbNode == null)
            {
                reverbNode = audioGraph.CreateSubmixNode();
            }
            if (eqNode == null)
            {
                eqNode = audioGraph.CreateSubmixNode();
            }
            if (limiterNode == null)
            {
                limiterNode = audioGraph.CreateSubmixNode();
            }

            reverbNode.EffectDefinitions.Clear();
            eqNode.EffectDefinitions.Clear();
            limiterNode.EffectDefinitions.Clear();

            if (e == Effects.Equalizer)
            {
                reverbNode.EffectDefinitions.Add(eqEffect);
            }
            if (e == Effects.Limiter)
            {
                reverbNode.EffectDefinitions.Add(limiterEffect);
            }
            if (e == Effects.Reverb)
            {
                reverbNode.EffectDefinitions.Add(reverbEffect);
            }
        }

        private void ShutdownEffects()
        {
            fileInputNode.RemoveOutgoingConnection(limiterNode);
            limiterNode.RemoveOutgoingConnection(eqNode);
            eqNode.RemoveOutgoingConnection(reverbNode);
            reverbNode.RemoveOutgoingConnection(deviceOutputNode);

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
            //deviceOutputNode.OutgoingGain = vol;
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
            if (songs.IsNullorEmpty())
            {
                throw new ArgumentException("songs is empty");
            }
            files.Clear();
            currentPlayList.Clear();
            currentPlayList.AddRange(songs);

            foreach (var item in songs)
            {
                files.Add(await StorageFile.GetFileFromPathAsync(item.FilePath));
            }
            if (startIndex < 0)
                startIndex = 0;
            if (startIndex >= files.Count)
                startIndex = files.Count - 1;
            if (files.IsNullorEmpty())
            {
                throw new ArgumentException("Files empty");
            }
            currentIndex = startIndex;

            prepared = false;

            Play();
        }

        public async void Next()
        {
            DisconnectFromGraph();
            if (files.Count > currentIndex + 1)
            {
                previousFileNode?.Dispose();
                previousFileNode = null;

                previousFileNode = fileInputNode;
                fileInputNode = nextFileNode;
                nextFileNode = await CreateFileInputNodeAsync(files[currentIndex + 1]);

                //nextFileNode.StartTime = TimeSpan.Zero;
                //previousFileNode.StartTime = TimeSpan.Zero;

                currentIndex++;

                ConnectIntoGraph();
                audioGraph.Start();

                StatusChange(Windows.Media.Playback.MediaPlaybackState.Playing, true);
            }
            else if (isLoop)
            {

            }
        }

        private void ConnectIntoGraph()
        {
            fileInputNode.AddOutgoingConnection(limiterNode);
        }

        private void DisconnectFromGraph()
        {
            if (fileInputNode != null)
                fileInputNode.RemoveOutgoingConnection(limiterNode);
        }

        public void Pause()
        {
            lock (lockable)
            {
                currentPosition = fileInputNode?.Position;
                audioGraph.Stop();
                isPlaying = false;
                StatusChange(Windows.Media.Playback.MediaPlaybackState.Paused, false);
            }
        }

        public void Play()
        {
            lock (lockable)
            {
                if (prepared)
                {
                    //fileInputNode.StartTime = currentPosition;
                }
                else
                {
                    PrepareToPlay();
                }
                audioGraph.Start();
                isPlaying = true;

                StatusChange(Windows.Media.Playback.MediaPlaybackState.Playing, true);

                updateTimer?.Cancel();
                updateTimer = ThreadPoolTimer.CreatePeriodicTimer(UpdatePosition, TimeSpan.FromMilliseconds(50));
            }
        }

        private void UpdatePosition(ThreadPoolTimer timer)
        {
            try
            {
                if (fileInputNode != null)
                    PositionUpdated?.Invoke(this, new PositionUpdatedArgs()
                    {
                        Current = fileInputNode.Position,
                        Total = fileInputNode.Duration
                    });
            }
            catch (Exception)
            {
            }
        }

        private void StatusChange(Windows.Media.Playback.MediaPlaybackState state, bool b)
        {
            if (b)
            {
                StatusChanged?.Invoke(this, new StatusChangedArgs
                {
                    CurrentIndex = (uint)currentIndex,
                    IsLoop = isLoop,
                    IsShuffle = isShuffle,
                    CurrentSong = currentPlayList[currentIndex],
                    Items = currentPlayList,
                    State = state
                });
            }
            else
            {
                StatusChanged?.Invoke(this, new StatusChangedArgs
                {
                    IsLoop = isLoop,
                    IsShuffle = isShuffle,
                    State = state
                });
            }
        }

        private async void PrepareToPlay()
        {
            await PrepareNodes();

            //if (currentIndex >= 0 && currentIndex < files.Count && fileInputNode != null && currentPosition is TimeSpan t)
            //{
            //    fileInputNode.StartTime = t;
            //}
            if (!graphBuilt)
            {
                BuildGraph();
            }
        }

        private async Task PrepareNodes()
        {
            if (prepared)
                return;

            if (deviceOutputNode == null)
            {
                await CreateDeviceOutputNodeAsync();
            }

            fileInputNode?.Dispose();
            fileInputNode = null;
            nextFileNode?.Dispose();
            nextFileNode = null;
            previousFileNode?.Dispose();
            previousFileNode = null;

            if (files.IsNullorEmpty() || currentIndex < 0 || currentIndex >= files.Count)
            {
                throw new ArgumentException("Files empty");
            }
            fileInputNode = await CreateFileInputNodeAsync(files[currentIndex]);


            if (currentIndex > 0)
            {
                previousFileNode = await CreateFileInputNodeAsync(files[currentIndex - 1]);
            }
            if (currentIndex < files.Count - 1)
            {
                nextFileNode = await CreateFileInputNodeAsync(files[currentIndex + 1]);
            }

            if (reverbNode == null)
            {
                var settings = Settings.Load();
                ApplyEffects(settings.AudioGraphEffects);
            }

            prepared = true;
        }



        private void BuildGraph()
        {
            fileInputNode.AddOutgoingConnection(limiterNode);
            limiterNode.AddOutgoingConnection(eqNode);
            eqNode.AddOutgoingConnection(reverbNode);
            reverbNode.AddOutgoingConnection(deviceOutputNode);
            graphBuilt = true;
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

    }

}
