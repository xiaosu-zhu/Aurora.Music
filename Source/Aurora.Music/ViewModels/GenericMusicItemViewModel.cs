// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Music.Core.Storage;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using Aurora.Shared.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Aurora.Music.ViewModels
{
    public class GenericMusicItemViewModel : ViewModelBase
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Addtional { get; set; }

        public Visibility SearchDeleteVis(string s)
        {
            return s == "\uE16D" ? Visibility.Collapsed : Visibility.Visible;
        }

        public string AddtionalAndDescription()
        {
            return $"{Addtional} · {Description}";
        }

        public Uri Artwork
        { get; set; }

        private bool isOnline;
        public bool IsOnline
        {
            get { return isOnline; }
            set { SetProperty(ref isOnline, value); }
        }

        private bool isAvaliable;
        public bool IsAvaliable
        {
            get { return isAvaliable; }
            set { SetProperty(ref isAvaliable, value); }
        }

        public Color MainColor { get; set; }

        public int[] IDs { get; set; }
        public string[] OnlineIDs { get; set; }
        public string OnlineAlbumID { get; set; }
        public int ContextualID { get; set; }


        public bool IsSearch { get; set; }

        public GenericMusicItemViewModel()
        {

        }

        public string OnlineToSymbol(bool b)
        {
            return b ? "\uE753" : "\uEC4F";
        }

        public MediaType InnerType { get; set; }

        public SolidColorBrush GetMainColorBrush(double d)
        {
            MainColor.ColorToHSV(out var h, out var s, out var v);
            v *= d;

            return new SolidColorBrush(ImagingHelper.ColorFromHSV(h, s, v));
        }

        public Color GetMainColor(double d)
        {
            MainColor.ColorToHSV(out var h, out var s, out var v);
            v *= d;

            return ImagingHelper.ColorFromHSV(h, s, v);
        }

        public GenericMusicItemViewModel(Album album)
        {
            InnerType = MediaType.Album;
            ContextualID = album.ID;
            Title = album.Name;
            Addtional = string.Join(Consts.CommaSeparator, album.AlbumArtists);
            Description = SmartFormat.Smart.Format(Consts.Localizer.GetString("SmartSongs"), (album.Songs.Length + album.Songs.Length));
            Artwork = album.PicturePath.IsNullorEmpty() ? null : new Uri(album.PicturePath);
            IDs = album.Songs;
        }

        public GenericMusicItemViewModel(Song song)
        {
            InnerType = MediaType.Song;
            ContextualID = song.ID;
            Title = song.Title;
            Addtional = song.Performers.IsNullorEmpty() ? Consts.UnknownArtists : string.Join(Consts.CommaSeparator, song.Performers);
            Description = song.Album;
            Artwork = song.PicturePath.IsNullorEmpty() ? null : new Uri(song.PicturePath);
            IDs = new int[] { song.ID };
        }

        public GenericMusicItemViewModel(GenericMusicItem item)
        {
            // TODO: online pic path
            if (item is OnlineMusicItem o)
            {
                IsOnline = true;
                OnlineIDs = o.OnlineID;
                OnlineAlbumID = o.OnlineAlbumId;
            }
            else
            {
                ContextualID = item.ContextualID;
                IDs = item.IDs;
            }
            InnerType = item.InnerType;
            Title = item.Title;
            Description = item.Description;
            Addtional = item.Addtional;
            Artwork = item.PicturePath.IsNullorEmpty() ? null : new Uri(item.PicturePath);
        }

        internal virtual async Task<IList<Song>> GetSongsAsync()
        {
            if (IsOnline)
            {
                if (MainPageViewModel.Current.OnlineMusicExtension == null)
                {
                    return null;
                }
                var list = new List<Song>();
                foreach (var item in OnlineIDs)
                {
                    var s = await MainPageViewModel.Current.GetOnlineSongAsync(item);
                    if (s == null)
                        continue;
                    list.Add(s);
                }
                return list;
            }
            else
            {
                if (IDs == null)
                {
                    return new List<Song>();
                }
                var opr = SQLOperator.Current();

                var s = await opr.GetSongsAsync(IDs);
                var s1 = s.OrderBy(x => x.Track);
                s1 = s1.OrderBy(x => x.Disc);
                return s1.ToList();
            }
        }

        public override string ToString()
        {
            if (Title.IsNullorEmpty())
            {
                return string.Empty;
            }

            if (InnerType == MediaType.Placeholder)
            {
                return Title;
            }

            if (Description.IsNullorEmpty())
            {
                return Title;
            }
            var title = Title;
            if (title.Length > 20)
            {
                title = title.Substring(0, 20);
                title += "…";
            }
            return $"{title} - {Description}";
        }

        internal async Task<AlbumViewModel> FindAssociatedAlbumAsync()
        {
            var opr = SQLOperator.Current();
            switch (InnerType)
            {
                case MediaType.Song:
                    if (Description.IsNullorEmpty())
                    {
                        return null;
                    }
                    if (IsOnline)
                    {
                        if (OnlineAlbumID.IsNullorEmpty())
                            return null;
                        return new AlbumViewModel(await MainPageViewModel.Current.GetOnlineAlbumAsync(OnlineAlbumID));
                    }
                    return new AlbumViewModel(await opr.GetAlbumByNameAsync(Description, ContextualID));
                case MediaType.Album:
                    if (ContextualID == default(int))
                    {
                        return new AlbumViewModel(await opr.GetAlbumByNameAsync(Title));
                    }
                    return new AlbumViewModel(await opr.GetAlbumByIDAsync(ContextualID));
                case MediaType.PlayList:
                    throw new NotImplementedException();
                case MediaType.Artist:
                    throw new InvalidCastException("This GenericMusicItemViewModel is artist");
                default:
                    return null;
            }
        }
    }

    public class HeroItemViewModel : GenericMusicItemViewModel
    {
        public Uri Artwork1 { get; set; }
        public Uri Artwork2 { get; set; }
        public Uri Artwork3 { get; set; }
        public Uri Artwork4 { get; set; }
        public Uri Artwork5 { get; set; }
        public Uri Artwork6 { get; set; }
        public Uri Artwork7 { get; set; }

        public Color MainColor1
        {
            get
            {
                MainColor.ColorToHSV(out var h, out var s, out var v);

                v = 1.25;
                s *= 1.25;
                if (v > 0.666666666667) v = 0.666666666667;
                if (s > 1) s = 1;

                return ImagingHelper.ColorFromHSV(h, s, v);
            }
        }
    }
}
