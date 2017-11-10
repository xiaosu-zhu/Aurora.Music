using Aurora.Music.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        event EventHandler<PositionUpdatedArgs> PositionUpdated;
        event EventHandler<StatusChangedArgs> StatusChanged;

    }
}
