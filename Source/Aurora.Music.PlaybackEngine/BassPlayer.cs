// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurora.Music.Core.Models;
using Windows.Storage;
using Un4seen.Bass;
using Windows.Storage.FileProperties;
using Windows.Media.MediaProperties;
using Windows.Media.Core;

namespace Aurora.Music.PlaybackEngine
{
    public class BassPlayer : IPlayer
    {
        public bool? IsPlaying => throw new NotImplementedException();

        public static BassPlayer Current => throw new NotImplementedException();

        public BassPlayer()
        {
            BassNet.Registration("pkzxs123@gmail.com", "2X25201018152222");
            if (!Bass.BASS_Init(0, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero))
            {
                BASSError err = Bass.BASS_ErrorGetCode();
                return;
            }
            else
            {

            }
        }

        async Task InitializeAsync(Song s)
        {
            StorageFile sFile = await StorageFile.GetFileFromPathAsync(s.FilePath);
            BasicProperties prop = await sFile.GetBasicPropertiesAsync();
            var size = prop.Size;

            var m_BassHandle = Bass.BASS_StreamCreateFile(s.FilePath, 0, (long)size, BASSFlag.BASS_STREAM_DECODE);
            if (m_BassHandle == 0)
            {
                BASSError err = Bass.BASS_ErrorGetCode();
                System.Diagnostics.Debug.WriteLine("InitializeAsync error {0}", err);
            }

            BASS_CHANNELINFO cInfo = Bass.BASS_ChannelGetInfo(m_BassHandle);
            if (cInfo == null)
            {
                BASSError err = Bass.BASS_ErrorGetCode();
                System.Diagnostics.Debug.WriteLine("InitializeAsync error {0}", err);
            }
            long len = Bass.BASS_ChannelGetLength(m_BassHandle, BASSMode.BASS_POS_BYTES);
            double secs = Bass.BASS_ChannelBytes2Seconds(m_BassHandle, len);
            uint bits = 16;
            if (cInfo.Is32bit)
                bits = 32;
            else if (cInfo.Is8bit)
                bits = 8;

            AudioEncodingProperties pcmprops = AudioEncodingProperties.CreatePcm((uint)cInfo.freq, (uint)cInfo.chans, bits);
            var m_MediaStreamSource = new MediaStreamSource(new AudioStreamDescriptor(pcmprops))
            {
                CanSeek = true,
                BufferTime = TimeSpan.Zero,
                Duration = TimeSpan.FromSeconds(secs)
            };

            // play the stream channel
            Bass.BASS_ChannelPlay(m_BassHandle, false);
        } // InitializeAsync

        public async Task Play(StorageFile s)
        {
            BasicProperties prop = await s.GetBasicPropertiesAsync();
            var m_BassHandle = Bass.BASS_StreamCreateFile(s.Path, 0, (long)prop.Size, BASSFlag.BASS_STREAM_DECODE);
            if (m_BassHandle == 0)
            {
                BASSError err = Bass.BASS_ErrorGetCode();
                System.Diagnostics.Debug.WriteLine("InitializeAsync error {0}", err);
            }
            Bass.BASS_ChannelPlay(m_BassHandle, false);
        }

        private void M_MediaStreamSource_SampleRequested(MediaStreamSource sender, MediaStreamSourceSampleRequestedEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void M_MediaStreamSource_Starting(MediaStreamSource sender, MediaStreamSourceStartingEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void M_MediaStreamSource_Closed(MediaStreamSource sender, MediaStreamSourceClosedEventArgs args)
        {
            throw new NotImplementedException();
        }

        public event EventHandler<PositionUpdatedArgs> PositionUpdated;
        public event EventHandler<PlayingItemsChangedArgs> StatusChanged;
        public event EventHandler<DownloadProgressChangedArgs> DownloadProgressChanged;

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

        public Task NewPlayList(IList<Song> songs, int startIndex = 0)
        {
            throw new NotImplementedException();
        }

        public Task NewPlayList(IList<StorageFile> list)
        {
            throw new NotImplementedException();
        }

        public void Next()
        {
            throw new NotImplementedException();
        }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Play()
        {
            throw new NotImplementedException();
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

        public void SkiptoIndex(uint index)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public Task UpdateComingItems(List<Song> list)
        {
            throw new NotImplementedException();
        }
    }
}
