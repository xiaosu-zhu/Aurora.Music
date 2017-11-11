using Aurora.Music.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Playback;

namespace Aurora.Music.Core.Player
{
    public interface IPlayer
    {
        void Play();
        void Pause();
        void Stop();
        void Seek(TimeSpan position);
        void Next();
        void Previous();
        Task NewPlayList(IList<Song> songs, int startIndex = 0);
        void ChangeVolume(double vol);
        void ChangeAudioEndPoint(string outputDeviceID);
        void Loop(bool? isOn);
        void Shuffle(bool? isOn);

        bool? IsPlaying { get; }

        event EventHandler<PositionUpdatedArgs> PositionUpdated;
        event EventHandler<StatusChangedArgs> StatusChanged;

    }


    public class StatusChangedArgs
    {
        public Song CurrentSong { get; set; }
        public MediaPlaybackState State { get; set; }
        public bool IsLoop { get; set; }
        public bool IsShuffle { get; set; }

        public uint CurrentIndex { get; set; }
        public IReadOnlyList<Song> Items { get; internal set; }
    }

    public class PositionUpdatedArgs
    {
        public TimeSpan Current { get; set; }
        public TimeSpan Total { get; set; }
    }
}
