using Aurora.Music.Core.Models;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Media.Audio;
using Windows.Storage;
using Windows.System.Threading;

namespace Aurora.Music.PlaybackEngine
{
    public sealed class AudioGraphPlayer : IPlayer, IDisposable
    {
        private static object lockable = new object();



        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        private void Dispose(bool disposing)
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
        public event EventHandler<PlayingItemsChangedArgs> StatusChanged;
        public event EventHandler<DownloadProgressChangedArgs> DownloadProgressChanged;

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
        private bool connected;

        public async Task Init()
        {
            audioSettings = new AudioGraphSettings(Windows.Media.Render.AudioRenderCategory.Media);

            ChangeAudioEndPoint(Settings.Current.OutputDeviceID);

            CreateAudioGraphResult result = await AudioGraph.CreateAsync(audioSettings);
            if (result.Status != AudioGraphCreationStatus.Success)
            {
                throw new InvalidOperationException("Can't create AudioGraph");
            }

            audioGraph = result.Graph;
            ChangeVolume(Settings.Current.PlayerVolume);
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

        private void InitEffects()
        {
            reverbEffect = new ReverbEffectDefinition(audioGraph)
            {
                WetDryMix = 50,
                ReflectionsDelay = 12,
                ReverbDelay = 30,
                RearDelay = 3,
                DecayTime = 2,
            };
            limiterEffect = new LimiterEffectDefinition(audioGraph)
            {
                Loudness = 1000,
                Release = 10
            };
            eqEffect = new EqualizerEffectDefinition(audioGraph);


            eqEffect.Bands[0].FrequencyCenter = 150.0f;
            eqEffect.Bands[0].Gain = 4.033f;
            eqEffect.Bands[0].Bandwidth = 2f;

            eqEffect.Bands[1].FrequencyCenter = 300.0f;
            eqEffect.Bands[1].Gain = 1.6888f;
            eqEffect.Bands[1].Bandwidth = 2f;

            eqEffect.Bands[2].FrequencyCenter = 6000.0f;
            eqEffect.Bands[2].Gain = 2.4702f;
            eqEffect.Bands[2].Bandwidth = 2f;

            eqEffect.Bands[3].FrequencyCenter = 12000.0f;
            eqEffect.Bands[3].Gain = 5.5958f;
            eqEffect.Bands[3].Bandwidth = 2f;
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

            if (eqEffect == null)
            {
                InitEffects();
            }

            reverbNode.EffectDefinitions.Clear();
            eqNode.EffectDefinitions.Clear();
            limiterNode.EffectDefinitions.Clear();



            if (e.HasFlag(Effects.Equalizer))
            {
                eqNode.EffectDefinitions.Add(eqEffect);
                eqNode.EnableEffectsByDefinition(eqEffect);
            }
            if (e.HasFlag(Effects.Limiter))
            {
                limiterNode.EffectDefinitions.Add(limiterEffect);
                limiterNode.EnableEffectsByDefinition(limiterEffect);
            }
            if (e.HasFlag(Effects.Reverb))
            {
                reverbNode.EffectDefinitions.Add(reverbEffect);
                reverbNode.EnableEffectsByDefinition(reverbEffect);
            }
        }

        private void ShutdownEffects()
        {
            eqNode.DisableEffectsByDefinition(eqEffect);
            limiterNode.DisableEffectsByDefinition(limiterEffect);
            reverbNode.DisableEffectsByDefinition(reverbEffect);
            reverbNode.EffectDefinitions.Clear();
            eqNode.EffectDefinitions.Clear();
            limiterNode.EffectDefinitions.Clear();
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
            if (isOn is bool b && b)
            {
                isLoop = true;

            }
            else
            {
                isLoop = false;
            }

            StatusChange(false);
        }

        public async Task NewPlayList(IList<Song> songs, int startIndex = 0)
        {
            Stop();
            CleanUp();

            if (songs.IsNullorEmpty())
            {
                throw new ArgumentException("songs is empty");
            }
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

        private void CleanUp()
        {
            nextFileNode?.Dispose();
            nextFileNode = null;
            fileInputNode?.Dispose();
            fileInputNode = null;
            previousFileNode?.Dispose();
            previousFileNode = null;

            files.Clear();
            currentPlayList.Clear();
        }

        public async void Next()
        {
            if (fileInputNode == null)
            {
                isPlaying = false;
                return;
            }

            DisconnectFromGraph();
            if (files.Count > currentIndex)
            {
                currentIndex++;

                previousFileNode?.Dispose();
                previousFileNode = null;

                previousFileNode = fileInputNode;
                fileInputNode = nextFileNode;
                nextFileNode = files.Count > currentIndex + 1 ? await CreateFileInputNodeAsync(files[currentIndex + 1]) : null;

                //nextFileNode.StartTime = TimeSpan.Zero;
                //previousFileNode.StartTime = TimeSpan.Zero;
            }
            else if (isLoop)
            {
                // TODO
            }
            else
            {
                audioGraph.Stop();
                currentIndex = -1;
            }
            if (isPlaying is bool b && b)
            {
                Play();
            }
            else
            {
                Pause();
            }
        }


        public async void Previous()
        {
            if (fileInputNode == null)
            {
                isPlaying = false;
                return;
            }

            if (fileInputNode.Position.TotalSeconds > 3)
            {
                Seek(TimeSpan.Zero);
                return;
            }

            DisconnectFromGraph();


            if (currentIndex > 0)
            {
                currentIndex--;

                nextFileNode?.Dispose();
                nextFileNode = null;

                nextFileNode = fileInputNode;
                fileInputNode = previousFileNode;
                previousFileNode = currentIndex > 0 ? await CreateFileInputNodeAsync(files[currentIndex - 1]) : null;

                //nextFileNode.StartTime = TimeSpan.Zero;
                //previousFileNode.StartTime = TimeSpan.Zero;
            }
            else if (isLoop)
            {
                // TODO
            }
            else
            {
                audioGraph.Stop();
                currentIndex = -1;
            }
            if (isPlaying is bool b && b)
            {
                Play();
            }
            else
            {
                Pause();
            }
        }

        private void ConnectIntoGraph()
        {
            if (connected)
            {
                return;
            }
            fileInputNode.AddOutgoingConnection(limiterNode);
            fileInputNode.FileCompleted += FileInputNode_FileCompleted;
            connected = true;
        }

        private void DisconnectFromGraph()
        {
            if (fileInputNode != null)
            {
                fileInputNode.FileCompleted -= FileInputNode_FileCompleted;
                fileInputNode.RemoveOutgoingConnection(limiterNode);
            }
            connected = false;
        }

        private void FileInputNode_FileCompleted(AudioFileInputNode sender, object args)
        {
            Next();
        }

        public void Pause()
        {
            lock (lockable)
            {
                currentPosition = fileInputNode?.Position;
                audioGraph.Stop();
                isPlaying = false;
                StatusChange(false);
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
                    AsyncHelper.RunSync(async () => await PrepareToPlay());
                }
                ConnectIntoGraph();

                audioGraph.Start();
                isPlaying = true;

                StatusChange(true);

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

        private void StatusChange(bool b)
        {
            if (b)
            {
                StatusChanged?.Invoke(this, new PlayingItemsChangedArgs
                {
                    CurrentIndex = currentIndex,
                    IsLoop = isLoop,
                    IsShuffle = isShuffle,
                    CurrentSong = currentPlayList[currentIndex],
                    Items = currentPlayList,
                });
            }
            else
            {
                StatusChanged?.Invoke(this, new PlayingItemsChangedArgs
                {
                    IsLoop = isLoop,
                    IsShuffle = isShuffle,
                });
            }
        }

        private async Task PrepareToPlay()
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
                ApplyEffects(Settings.Current.AudioGraphEffects);
            }

            prepared = true;
        }



        private void BuildGraph()
        {
            limiterNode.AddOutgoingConnection(eqNode);
            eqNode.AddOutgoingConnection(reverbNode);
            reverbNode.AddOutgoingConnection(deviceOutputNode);
            graphBuilt = true;
        }


        public void Seek(TimeSpan position)
        {
            fileInputNode?.Seek(position);
        }

        public void Shuffle(bool? isOn)
        {
            if (isOn is bool b && b)
            {
                isShuffle = true;

            }
            else
            {
                isShuffle = false;
            }

            StatusChange(false);
        }

        public void Stop()
        {
            audioGraph.Stop();
            DisconnectFromGraph();
        }

        public void SkiptoIndex(uint index)
        {
            throw new NotImplementedException();
        }

        public Task ReloadCurrent()
        {
            throw new NotImplementedException();
        }

        public void RemoveCurrentItem()
        {
            throw new NotImplementedException();
        }

        public Task DetachCurrentItem()
        {
            throw new NotImplementedException();
        }

        public Task ReAttachCurrentItem()
        {
            throw new NotImplementedException();
        }

        public Task AddtoNextPlay(IList<Song> song)
        {
            throw new NotImplementedException();
        }

        public Task UpdateComingItems(List<Song> list)
        {
            throw new NotImplementedException();
        }

        public Task NewPlayList(IList<StorageFile> list)
        {
            throw new NotImplementedException();
        }
    }
}
