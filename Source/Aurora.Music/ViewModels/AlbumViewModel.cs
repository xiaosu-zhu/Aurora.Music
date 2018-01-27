// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
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
using Aurora.Music.Core;
using Windows.UI.Xaml.Media;

namespace Aurora.Music.ViewModels
{
    public class AlbumViewModel : ViewModelBase, IKey
    {
        public AlbumViewModel(Album album)
        {
            IsOnline = album.IsOnline;
            if (album.IsOnline)
            {
                OnlieIDs = album.OnlineIDs;
                Songs = album.SongItems?.ToList();
            }

            Description = album.Desription;

            SongsID = album.Songs;
            Name = album.Name;
            if (!album.PicturePath.IsNullorEmpty())
            {
                ArtworkUri = new Uri(album.PicturePath);
                Artwork = new BitmapImage(ArtworkUri)
                {
                    DecodePixelHeight = 160,
                    DecodePixelType = DecodePixelType.Logical
                };
            }
            Genres = album.Genres;
            Year = album.Year;
            AlbumSort = album.AlbumSort;
            TrackCount = album.TrackCount;
            DiscCount = album.DiscCount;
            AlbumArtists = album.AlbumArtists;
            AlbumArtistsSort = album.AlbumArtistsSort;
            ReplayGainAlbumGain = album.ReplayGainAlbumGain;
            ReplayGainAlbumPeak = album.ReplayGainAlbumPeak;
            ID = album.ID;
        }

        private bool isOnline;
        public bool IsOnline
        {
            get { return isOnline; }
            set { SetProperty(ref isOnline, value); }
        }

        public AlbumViewModel() { }

        public int[] SongsID { get; private set; }

        public List<Song> Songs { get; set; } = new List<Song>();

        private Uri artworkUri;
        public Uri ArtworkUri
        {
            get
            {
                return artworkUri;
            }
            set
            {
                artworkUri = value;
            }
        }


        private const string splitter = " · ";
        internal string GetBrief()
        {
            string b = "";
            if (Year != default(uint))
            {
                b += Year;
                b += splitter;
            }
            if (!Genres.IsNullorEmpty())
                foreach (var item in Genres)
                {
                    b += item;
                    b += splitter;
                }
            if (!Songs.IsNullorEmpty())
            {
                b += SmartFormat.Smart.Format(Consts.Localizer.GetString("SmartSongs"), Songs.Count);
            }
            else if (SongsID != null)
            {
                b += SmartFormat.Smart.Format(Consts.Localizer.GetString("SmartSongs"), SongsID.Length);
            }
            return b;
        }

        private BitmapImage artwork;
        public BitmapImage Artwork
        {
            get { return artwork; }
            set { SetProperty(ref artwork, value); }
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
            return AlbumArtists.IsNullorEmpty() ? Consts.UnknownArtists : string.Join(Consts.CommaSeparator, AlbumArtists);
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
                if (Name.StartsWith("The ", StringComparison.CurrentCultureIgnoreCase))
                {
                    return Name.Substring(4);
                }
                if (Name.StartsWith("A ", StringComparison.CurrentCultureIgnoreCase))
                {
                    return Name.Substring(2);
                }
                if (Name.StartsWith("An ", StringComparison.CurrentCultureIgnoreCase))
                {
                    return Name.Substring(3);
                }
                return Name;

            }
        }

        private string description;
        public string Description
        {
            get { return description; }
            set { SetProperty(ref description, value); }
        }

        public string[] OnlieIDs { get; }

        internal async Task<List<Song>> GetSongsAsync()
        {
            if (IsOnline && Songs != null && Songs.Count > 0)
            {
                return Songs;
            }
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
                        Name = Name ?? string.Empty,
                        Genres = Genres ?? new string[] { },
                        Year = Year,
                        AlbumSort = AlbumSort ?? string.Empty,
                        TrackCount = TrackCount,
                        DiscCount = DiscCount,
                        AlbumArtists = AlbumArtists ?? new string[] { },
                        AlbumArtistsSort = AlbumArtistsSort ?? new string[] { },
                        ReplayGainAlbumGain = ReplayGainAlbumGain,
                        ReplayGainAlbumPeak = ReplayGainAlbumPeak,
                        PicturePath = ArtworkUri == null ? string.Empty : ArtworkUri.AbsolutePath ?? string.Empty,
                    });
                }
            });

            return s1.ToList();
        }
    }
}
