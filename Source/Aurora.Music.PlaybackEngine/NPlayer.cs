// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core.Models;
using NAudio.CoreAudioApi;
using NAudio.MediaFoundation;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Aurora.Music.PlaybackEngine
{
    public class NPlayer : IPlayer, IDisposable
    {
        private IWavePlayer player;
        private WaveStream reader;

        private readonly List<Song> playingList = new List<Song>();
        private readonly List<string> tokens = new List<string>();

        public bool? IsPlaying => throw new NotImplementedException();

        public double PlaybackRate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public double Volume => throw new NotImplementedException();

        public MediaPlayer MediaPlayer => throw new NotImplementedException();

        public event EventHandler<PositionUpdatedArgs> PositionUpdated;
        public event EventHandler<PlayingItemsChangedArgs> ItemsChanged;
        public event EventHandler<DownloadProgressChangedArgs> DownloadProgressChanged;
        public event EventHandler<PlaybackStatusChangedArgs> PlaybackStatusChanged;

        public NPlayer()
        {
            player = new WasapiOutRT(AudioClientShareMode.Shared, 200);
        }

        public Task AddtoNextPlay(IList<Song> song)
        {
            throw new NotImplementedException();
        }

        public void ChangeAudioEndPoint(string outputDeviceID)
        {
            throw new NotImplementedException();
        }

        public void ChangeVolume(double vol)
        {
            throw new NotImplementedException();
        }

        public Task DetachCurrentItem()
        {
            throw new NotImplementedException();
        }

        public void Loop(bool? isOn)
        {
            throw new NotImplementedException();
        }

        public async Task NewPlayList(IList<Song> songs, int startIndex = 0)
        {
            playingList.Clear();
            playingList.AddRange(songs);

            List<StorageFile> files = new List<StorageFile>();

            var f = await StorageFile.GetFileFromPathAsync(songs[0].FilePath);

            var stream = await f.OpenAsync(FileAccessMode.Read);
            if (stream == null) return;

            player.Init(() => CreateReader(new IRandomAccessStream[] { stream }));
        }

        private IWaveProvider CreateReader(IRandomAccessStream[] streams)
        {
            return new WaveController(streams.Select(a => new MediaFoundationReaderUniversal(a)));
        }

        public Task NewPlayList(IList<StorageFile> list, int startIndex = 0)
        {
            throw new NotImplementedException();
        }

        public void Next()
        {
            throw new NotImplementedException();
        }

        public void Pause()
        {
            player.Pause();
        }

        public void Play()
        {
            player.Play();
        }

        public void Previous()
        {
            throw new NotImplementedException();
        }

        public Task ReAttachCurrentItem()
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

        public void Seek(TimeSpan position)
        {
            throw new NotImplementedException();
        }

        public void Shuffle(bool? isOn)
        {
            throw new NotImplementedException();
        }

        public void SkiptoIndex(int index)
        {
            throw new NotImplementedException();
        }

        public void Stop()
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
        // ~NPlayer() {
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

        public void ChangeEQ(float[] gain)
        {
            throw new NotImplementedException();
        }

        public void ToggleEffect(Core.Models.Effects audioGraphEffects)
        {
            throw new NotImplementedException();
        }

        public void Backward(TimeSpan timeSpan)
        {
            throw new NotImplementedException();
        }

        public void Forward(TimeSpan timeSpan)
        {
            throw new NotImplementedException();
        }

        public void RefreshNowPlayingInfo()
        {
            throw new NotImplementedException();
        }
        #endregion
    }


    // Slightly hacky approach to supporting a different WinRT constructor
    class MediaFoundationReaderUniversal : MediaFoundationReader
    {
        private readonly MediaFoundationReaderUniversalSettings settings;

        public class MediaFoundationReaderUniversalSettings : MediaFoundationReaderSettings
        {
            public MediaFoundationReaderUniversalSettings()
            {
                // can't recreate since we're using a file stream
                SingleReaderObject = true;
            }

            public IRandomAccessStream Stream { get; set; }
        }

        public MediaFoundationReaderUniversal(IRandomAccessStream stream)
            : this(new MediaFoundationReaderUniversalSettings() { Stream = stream })
        {

        }


        public MediaFoundationReaderUniversal(MediaFoundationReaderUniversalSettings settings)
            : base(null, settings)
        {
            this.settings = settings;
        }

        protected override IMFSourceReader CreateReader(MediaFoundationReaderSettings settings)
        {
            var fileStream = ((MediaFoundationReaderUniversalSettings)settings).Stream;
            var byteStream = MediaFoundationApi.CreateByteStream(fileStream);
            var reader = MediaFoundationApi.CreateSourceReaderFromByteStream(byteStream);
            reader.SetStreamSelection(MediaFoundationInterop.MF_SOURCE_READER_ALL_STREAMS, false);
            reader.SetStreamSelection(MediaFoundationInterop.MF_SOURCE_READER_FIRST_AUDIO_STREAM, true);

            // Create a partial media type indicating that we want uncompressed PCM audio

            var partialMediaType = new NAudio.MediaFoundation.MediaType
            {
                MajorType = MediaTypes.MFMediaType_Audio,
                SubType = settings.RequestFloatOutput ? AudioSubtypes.MFAudioFormat_Float : AudioSubtypes.MFAudioFormat_PCM
            };

            // set the media type
            // can return MF_E_INVALIDMEDIATYPE if not supported
            reader.SetCurrentMediaType(MediaFoundationInterop.MF_SOURCE_READER_FIRST_AUDIO_STREAM, IntPtr.Zero, partialMediaType.MediaFoundationObject);
            return reader;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                settings.Stream?.Dispose();
            }
            base.Dispose(disposing);
        }
    }


    /// <summary>
    /// Stream for looping playback
    /// </summary>
    class LoopStream : WaveStream
    {
        WaveStream sourceStream;

        /// <summary>
        /// Creates a new Loop stream
        /// </summary>
        /// <param name="sourceStream">The stream to read from. Note: the Read method of this stream should return 0 when it reaches the end
        /// or else we will not loop to the start again.</param>
        public LoopStream(WaveStream sourceStream)
        {
            this.sourceStream = sourceStream;
            this.EnableLooping = true;
        }

        /// <summary>
        /// Use this to turn looping on or off
        /// </summary>
        public bool EnableLooping { get; set; }

        /// <summary>
        /// Return source stream's wave format
        /// </summary>
        public override WaveFormat WaveFormat
        {
            get { return sourceStream.WaveFormat; }
        }

        /// <summary>
        /// LoopStream simply returns
        /// </summary>
        public override long Length
        {
            get { return sourceStream.Length; }
        }

        /// <summary>
        /// LoopStream simply passes on positioning to source stream
        /// </summary>
        public override long Position
        {
            get { return sourceStream.Position; }
            set { sourceStream.Position = value; }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int totalBytesRead = 0;

            while (totalBytesRead < count)
            {
                int bytesRead = sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                if (bytesRead == 0)
                {
                    if (sourceStream.Position == 0 || !EnableLooping)
                    {
                        // something wrong with the source stream
                        break;
                    }
                    // loop
                    sourceStream.Position = 0;
                }
                totalBytesRead += bytesRead;
            }
            return totalBytesRead;
        }
    }

    internal static class Helper
    {
        public static WaveController FollowedBy(this WaveStream a, WaveStream b)
        {
            return new WaveController(new WaveStream[] { a, b });
        }
    }

    /// <summary>
    /// Sample Provider to concatenate multiple sample providers together
    /// </summary>
    class WaveController : WaveStream
    {
        private readonly List<WaveStream> providers;
        private int currentProviderIndex;

        /// <summary>
        /// Creates a new ConcatenatingSampleProvider
        /// </summary>
        /// <param name="providers">The source providers to play one after the other. Must all share the same sample rate and channel count</param>
        public WaveController(IEnumerable<WaveStream> providers)
        {
            if (providers == null) throw new ArgumentNullException(nameof(providers));
            this.providers = new List<WaveStream>(providers);
            if (this.providers.Count == 0) throw new ArgumentException("Must provide at least one input", nameof(providers));
            if (this.providers.Any(p => p.WaveFormat.Channels != WaveFormat.Channels)) throw new ArgumentException("All inputs must have the same channel count", nameof(providers));
            if (this.providers.Any(p => p.WaveFormat.SampleRate != WaveFormat.SampleRate)) throw new ArgumentException("All inputs must have the same sample rate", nameof(providers));
        }

        /// <summary>
        /// The WaveFormat of this Sample Provider
        /// </summary>
        public override WaveFormat WaveFormat => providers[0].WaveFormat;

        public override long Length => providers[currentProviderIndex].Length;

        public override long Position { get => providers[currentProviderIndex].Position; set => providers[currentProviderIndex].Position = value; }

        /// <summary>
        /// Read Samples from this sample provider
        /// </summary>
        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = 0;
            while (read < count && currentProviderIndex < providers.Count)
            {
                var needed = count - read;
                var readThisTime = providers[currentProviderIndex].Read(buffer, read, needed);
                read += readThisTime;
                if (readThisTime == 0) currentProviderIndex++;
            }
            return read;
        }
    }
}
