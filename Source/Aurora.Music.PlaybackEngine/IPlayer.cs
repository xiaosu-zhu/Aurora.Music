using Aurora.Music.Core.Models;
using Aurora.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Aurora.Music.PlaybackEngine
{
    public interface IPlayer : IDisposable
    {
        [UriActivate("?action=play", Usage = ActivateUsage.Query)]
        void Play();
        [UriActivate("?action=pause", Usage = ActivateUsage.Query)]
        void Pause();
        [UriActivate("?action=stop", Usage = ActivateUsage.Query)]
        void Stop();
        void Seek(TimeSpan position);
        [UriActivate("?action=next", Usage = ActivateUsage.Query)]
        void Next();
        [UriActivate("?action=previous", Usage = ActivateUsage.Query)]
        void Previous();
        Task NewPlayList(IList<Song> songs, int startIndex = 0);
        Task NewPlayList(IList<StorageFile> list, int startIndex = 0);
        Task AddtoNextPlay(IList<Song> song);
        void ChangeVolume(double vol);
        void ChangeAudioEndPoint(string outputDeviceID);
        void Loop(bool? isOn);
        void Shuffle(bool? isOn);

        MediaPlayer MediaPlayer { get; }

        bool? IsPlaying { get; }
        double PlaybackRate { get; set; }
        double Volume { get; }

        event EventHandler<PositionUpdatedArgs> PositionUpdated;
        event EventHandler<PlayingItemsChangedArgs> ItemsChanged;
        event EventHandler<PlaybackStatusChangedArgs> PlaybackStatusChanged;
        event EventHandler<DownloadProgressChangedArgs> DownloadProgressChanged;

        void SkiptoIndex(int index);
        Task ReloadCurrent();
        void RemoveCurrentItem();
        Task DetachCurrentItem();
        Task ReAttachCurrentItem();
        void ChangeEQ(float[] gain);
        void ToggleEffect(Core.Models.Effects audioGraphEffects);
        void Backward(TimeSpan timeSpan);
        void Forward(TimeSpan timeSpan);
        void RefreshNowPlayingInfo();
    }

    public class DownloadProgressChangedArgs
    {
        /// <summary>
        /// 0.0 to 1.0
        /// </summary>
        public double Progress { get; set; }
    }

    public class PlayingItemsChangedArgs
    {
        public Song CurrentSong { get; set; }
        public bool IsLoop { get; set; }
        public bool IsShuffle { get; set; }

        public bool IsOneLoop { get; set; }

        public RandomAccessStreamReference Thumnail { get; set; }

        public int CurrentIndex { get; set; }
        public IReadOnlyList<Song> Items { get; internal set; }
    }

    public class PlaybackStatusChangedArgs
    {
        public MediaPlaybackState PlaybackStatus { get; set; }
        public bool IsShuffle { get; set; }
        public bool IsLoop { get; set; }
        public bool IsOneLoop { get; set; }
    }

    public class PositionUpdatedArgs
    {
        public TimeSpan Current { get; set; }
        public TimeSpan Total { get; set; }
    }
}
