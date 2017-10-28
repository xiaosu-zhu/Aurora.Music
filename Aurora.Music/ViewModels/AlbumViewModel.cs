using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurora.Music.Core.Models;
using Aurora.Shared.Extensions;
using Aurora.Shared.MVVM;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections;
using Aurora.Music.Core.Storage;

namespace Aurora.Music.ViewModels
{
    class AlbumViewModel : ViewModelBase
    {
        public AlbumViewModel(Core.Models.Album item)
        {
            songs = item.Songs;
            Name = item.Name;
            if (!item.PicturePath.IsNullorEmpty())
            {
                Artwork = new Uri(item.PicturePath);
            }
            Genres = item.Genres;
            Year = item.Year;
            AlbumSort = item.AlbumSort;
            TrackCount = item.TrackCount;
            DiscCount = item.DiscCount;
            AlbumArtists = item.AlbumArtists;
            AlbumArtistsSort = item.AlbumArtistsSort;
            ReplayGainAlbumGain = item.ReplayGainAlbumGain;
            ReplayGainAlbumPeak = item.ReplayGainAlbumPeak;
        }

        public AlbumViewModel() { }

        private int[] songs;

        public ObservableCollection<SongViewModel> Songs { get; set; } = new ObservableCollection<SongViewModel>();

        public Uri Artwork { get; set; }

        public string Name { get; set; }
        public virtual string[] Genres { get; set; }
        public virtual uint Year { get; set; }
        public virtual string AlbumSort { get; set; }
        public virtual uint TrackCount { get; set; }
        public virtual uint DiscCount { get; set; }
        public virtual string[] AlbumArtists { get; set; }
        public virtual string[] AlbumArtistsSort { get; set; }
        public virtual double ReplayGainAlbumGain { get; set; }
        public virtual double ReplayGainAlbumPeak { get; set; }

        internal async Task<IEnumerable<Core.Storage.Song>> GetSongs()
        {
            var opr = SQLOperator.Current();
            return await opr.GetSongs(songs);
        }

        public BitmapImage ToImage(Uri r)
        {
            if (r != null)
                return new BitmapImage(r);
            else
                return null;
        }
    }
}
