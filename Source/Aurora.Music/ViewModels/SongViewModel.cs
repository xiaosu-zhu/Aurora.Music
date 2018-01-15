using System;
using Aurora.Music.Core.Storage;
using Aurora.Shared.Extensions;
using Aurora.Music.Core.Tools;
using Aurora.Music.Core;
using System.Linq;
using Aurora.Music.Core.Models;
using System.Collections.Generic;
using Aurora.Shared.MVVM;
using System.Threading.Tasks;
using Aurora.Shared.Helpers;

namespace Aurora.Music.ViewModels
{
    class SongViewModel : ViewModelBase, IKey
    {
        public SongViewModel()
        {

        }

        public bool IsOnline { get; set; }

        public string GetAddtionalDesc()
        {
            var descs = new List<string>();
            if (Song.Year != default(uint))
            {
                descs.Add(Song.Year.ToString());
            }
            if (!Song.Genres.IsNullorEmpty())
            {
                descs.AddRange(Song.Genres);
            }
            if (Song.BitRate != default(uint) && Song.BitRate != 0u)
            {
                descs.Add($"{(Song.BitRate / 1000.0).ToString("0.#", CultureInfoHelper.CurrentCulture)} Kbps");
            }
            if (Song.SampleRate != default(uint))
            {
                descs.Add($"{(Song.SampleRate / 1000.0).ToString("0.#", CultureInfoHelper.CurrentCulture)} KHz");
            }
            var type = GetFileType();
            if (!type.IsNullorEmpty())
                descs.Add(type);
            return string.Join(" · ", descs);
        }

        private string GetFileType()
        {
            if (IsOnline)
            {
                return "Online Content";
            }
            var ext = FilePath.Split('.').Last().ToLower();
            switch (ext)
            {
                case "mp3": return "MPEG-Layer 3";
                case "flac": return "Free Loseless";
                case "m4a":
                    if (Song.BitRate > 400 * 1000)
                    {
                        return "Apple Loseless";
                    }
                    else
                    {
                        return "Advanced Audio Coding";
                    }
                case "wav": return "Waveform";
                default: return null;
            }
        }

        public double Rating { get; set; }

        public SongViewModel(Song song)
        {
            Song = song;
            ID = song.ID;
            IsOnline = song.IsOnline;
            if (IsOnline)
            {
                FilePath = song.OnlineUri.AbsolutePath;
            }
            Rating = song.Rating;
            FilePath = song.FilePath;
            Album = song.Album;
            Title = song.Title;
            Track = song.Track;
            Artwork = new Uri(song.PicturePath.IsNullorEmpty() ? Consts.NowPlaceholder : song.PicturePath);
            Duration = song.Duration;
        }

        private TimeSpan duration;
        public TimeSpan Duration
        {
            get { return duration; }
            set { SetProperty(ref duration, value); }
        }

        public string StrArrtoDisplay(string[] arr)
        {
            if (arr.IsNullorEmpty())
            {
                return "Various";
            }
            else
            {
                return string.Join(", ", arr);
            }
        }

        public void RefreshFav()
        {
            if (IsOnline)
            {
                // TODO: Online fav
            }
            Favorite = AsyncHelper.RunSync(async () => await Song.GetFavoriteAsync());
        }

        public string DurationtoString(TimeSpan t)
        {
            return t.ToString($@"m\{CultureInfoHelper.CurrentCulture.DateTimeFormat.TimeSeparator}ss", CultureInfoHelper.CurrentCulture);
        }

        public string FormatDuration(TimeSpan t)
        {
            return t.GetSongDurationFormat();
        }

        public string GetIndex()
        {
            return (Index + 1).ToString();
        }

        public int ID { get; set; }
        public uint Index { get; set; }
        public string FilePath { get; set; }

        public Uri Artwork { get; set; }

        private Song song;
        public Song Song
        {
            get { return song; }
            set { SetProperty(ref song, value); }
        }

        internal string GetFormattedArtists()
        {
            return Song.AlbumArtists.IsNullorEmpty() ? Consts.UnknownArtists : string.Join(", ", Song.AlbumArtists);
        }

        private string title;
        public string Title
        {
            get { return title.IsNullorEmpty() ? FilePath.Split('\\').LastOrDefault() : title; }
            set { title = value; }
        }

        private string album;

        public string Album
        {
            get { return album.IsNullorEmpty() ? Consts.UnknownAlbum : album; }
            set { album = value; }
        }

        private bool fav;
        public bool Favorite
        {
            get { return fav; }
            set
            {
                SetProperty(ref fav, value);
                Song.WriteFav(value);
            }
        }

        internal async Task<AlbumViewModel> GetAlbumAsync()
        {
            if (IsOnline)
            {
                if (Song.OnlineAlbumID.IsNullorEmpty())
                    return null;
                return new AlbumViewModel(await MainPageViewModel.Current.GetOnlineAlbumAsync(Song.OnlineAlbumID));
            }
            return new AlbumViewModel(await SQLOperator.Current().GetAlbumByNameAsync(Song.Album, Song.ID));
        }

        private uint track;
        public uint Track
        {
            get
            {
                if (track == 0)
                    return Index + 1;
                return track;
            }
            set { track = value; }
        }

        public string FormattedAlbum
        {
            get
            {
                if (Album.StartsWith("The ", StringComparison.CurrentCultureIgnoreCase))
                {
                    return Album.Substring(4);
                }
                if (Album.StartsWith("A ", StringComparison.CurrentCultureIgnoreCase))
                {
                    return Album.Substring(2);
                }
                if (Album.StartsWith("An ", StringComparison.CurrentCultureIgnoreCase))
                {
                    return Album.Substring(3);
                }
                return Album;
            }
        }

        public string Key
        {
            get
            {
                if (Title.StartsWith("The ", StringComparison.CurrentCultureIgnoreCase))
                {
                    return Title.Substring(4);
                }
                if (Title.StartsWith("A ", StringComparison.CurrentCultureIgnoreCase))
                {
                    return Title.Substring(2);
                }
                if (Title.StartsWith("An ", StringComparison.CurrentCultureIgnoreCase))
                {
                    return Title.Substring(3);
                }
                return Title;

            }
        }
    }
}
