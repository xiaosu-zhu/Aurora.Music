using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Aurora.Shared.Extensions;
using Aurora.Shared.MVVM;
using Windows.UI.Xaml.Media.Imaging;
using Aurora.Music.Core.Storage;
using Aurora.Music.Core.Models;
using Windows.System.Threading;

namespace Aurora.Music.ViewModels
{
    class AlbumViewModel : ViewModelBase, IKey
    {
        public AlbumViewModel(Core.Models.Album item)
        {
            SongsID = item.Songs;
            Name = item.Name;
            if (!item.PicturePath.IsNullorEmpty())
            {
                artworkUri = new Uri(item.PicturePath);
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
            ID = item.ID;
        }

        public AlbumViewModel() { }

        public int[] SongsID { get; private set; }

        public List<Song> Songs { get; set; } = new List<Song>();

        private Uri artworkUri;
        public Uri Artwork
        {
            get
            {
                return artworkUri;
            }
            set
            {
                SetProperty(ref artworkUri, value);
            }
        }

        private string title;
        public string Name
        {
            get { return title; }
            set
            {
                SetProperty(ref title, value);
            }
        }

        public virtual string[] Genres { get; set; }
        public virtual uint Year { get; set; }
        public virtual string AlbumSort { get; set; }

        internal string GetFormattedArtists()
        {
            return AlbumArtists.IsNullorEmpty() ? "Unknown Artists" : string.Join(", ", AlbumArtists);
        }

        public virtual uint TrackCount { get; set; }
        public virtual uint DiscCount { get; set; }
        public virtual string[] AlbumArtists { get; set; }
        public virtual string[] AlbumArtistsSort { get; set; }
        public virtual double ReplayGainAlbumGain { get; set; }
        public virtual double ReplayGainAlbumPeak { get; set; }
        public int ID { get; }

        public string Key
        {
            get
            {
                if (Name.IsNullorEmpty())
                {
                    return " ";
                }
                if (Name.StartsWith("The ", System.StringComparison.CurrentCultureIgnoreCase))
                {
                    return Name.Substring(4);
                }
                if (Name.StartsWith("A ", System.StringComparison.CurrentCultureIgnoreCase))
                {
                    return Name.Substring(2);
                }
                if (Name.StartsWith("An ", System.StringComparison.CurrentCultureIgnoreCase))
                {
                    return Name.Substring(3);
                }
                return Name;

            }
        }

        internal async Task<List<Song>> GetSongsAsync()
        {
            if (Songs.Count == SongsID.Length)
            {
                return Songs;
            }
            Songs.Clear();
            var opr = SQLOperator.Current();
            var s = await opr.GetSongsAsync(SongsID);
            var s1 = s.OrderBy(x => x.Track);
            s1 = s1.OrderBy(x => x.Disc);
            Songs.AddRange(s1);

            var t = ThreadPool.RunAsync(async work =>
            {
                SongsID = s.Select(x => x.ID).ToArray();
                if (SongsID.IsNullorEmpty())
                {
                    await SQLOperator.Current().RemoveAlbumAsync(ID);
                }
                else
                {
                    await SQLOperator.Current().UpdateAlbumAsync(new Album(ID)
                    {
                        Songs = SongsID,
                        Name = Name,
                        Genres = Genres,
                        Year = Year,
                        AlbumSort = AlbumSort,
                        TrackCount = TrackCount,
                        DiscCount = DiscCount,
                        AlbumArtists = AlbumArtists,
                        AlbumArtistsSort = AlbumArtistsSort,
                        ReplayGainAlbumGain = ReplayGainAlbumGain,
                        ReplayGainAlbumPeak = ReplayGainAlbumPeak,
                        PicturePath = Artwork.AbsolutePath,
                    });
                }
            });

            return s1.ToList();
        }
    }
}
