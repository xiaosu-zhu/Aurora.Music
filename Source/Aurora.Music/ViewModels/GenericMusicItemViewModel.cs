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
    public class GenericMusicItemViewModel : ViewModelBase, IPreloadable<GenericMusicItemViewModel>
    {
        private string title;
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        private bool hasMultipleItem;
        public bool HasMultipleItem
        {
            get { return hasMultipleItem; }
            set { SetProperty(ref hasMultipleItem, value); }
        }

        private string description;
        public string Description
        {
            get { return description; }
            set { SetProperty(ref description, value); }
        }

        private string additional;
        public string Addtional
        {
            get { return additional; }
            set { SetProperty(ref additional, value); }
        }

        public Visibility SearchDeleteVis(string s)
        {
            return s == "\uE16D" ? Visibility.Collapsed : Visibility.Visible;
        }

        public string AddtionalAndDescription(string a, string b)
        {
            return $"{a} · {b}";
        }

        private Uri artwork;
        public Uri Artwork
        {
            get { return artwork; }
            set { SetProperty(ref artwork, value); }
        }

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

        private Color mainColor;
        public Color MainColor
        {
            get { return mainColor; }
            set { SetProperty(ref mainColor, value); }
        }

        public int[] IDs { get; set; }
        public string[] OnlineIDs { get; set; }
        public string OnlineAlbumID { get; set; }
        public int ContextualID { get; set; }


        public bool IsSearch { get; set; }

        public GenericMusicItemViewModel()
        {
            preloaded = false;
        }

        public string OnlineToSymbol(bool b)
        {
            return b ? "\uE753" : "\uEC4F";
        }

        public MediaType InnerType { get; set; }

        private bool preloaded = true;
        public bool Preloaded => preloaded;

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
            HasMultipleItem = IDs.Length > 1;
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
            HasMultipleItem = IDs.Length > 1;
        }

        public GenericMusicItemViewModel(GenericMusicItem item)
        {
            // TODO: online pic path
            if (item is OnlineMusicItem o)
            {
                IsOnline = true;
                OnlineIDs = o.OnlineID;
                OnlineAlbumID = o.OnlineAlbumId;
                if (OnlineIDs != null)
                    HasMultipleItem = o.OnlineAlbumId != null || OnlineIDs.Length > 1;
            }
            else
            {
                ContextualID = item.ContextualID;
                IDs = item.IDs;
                if (IDs != null)
                    HasMultipleItem = IDs.Length > 1;
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

        public void LoadWithActual(GenericMusicItemViewModel item)
        {
            Title = item.Title;
            Addtional = item.Addtional;
            Artwork = item.Artwork;
            ContextualID = item.ContextualID;
            Description = item.Description;
            IDs = item.IDs;
            InnerType = item.InnerType;
            IsAvaliable = item.IsAvaliable;
            IsOnline = item.IsOnline;
            IsSearch = item.IsSearch;
            MainColor = item.MainColor;
            OnlineAlbumID = item.OnlineAlbumID;
            OnlineIDs = item.OnlineIDs;
            if (IDs != null)
                HasMultipleItem = item.IDs.Length > 1;
            else if (OnlineIDs != null)
                HasMultipleItem = item.OnlineIDs.Length > 1;

            preloaded = true;
        }
    }

    public class HeroItemViewModel : GenericMusicItemViewModel, IPreloadable<HeroItemViewModel>
    {
        private Uri artwork1;
        public Uri Artwork1
        {
            get { return artwork1; }
            set { SetProperty(ref artwork1, value); }
        }
        private Uri artwork2;
        public Uri Artwork2
        {
            get { return artwork2; }
            set { SetProperty(ref artwork2, value); }
        }
        private Uri artwork3;
        public Uri Artwork3
        {
            get { return artwork3; }
            set { SetProperty(ref artwork3, value); }
        }
        private Uri artwork4;
        public Uri Artwork4
        {
            get { return artwork4; }
            set { SetProperty(ref artwork4, value); }
        }
        private Uri artwork5;
        public Uri Artwork5
        {
            get { return artwork5; }
            set { SetProperty(ref artwork5, value); }
        }
        private Uri artwork6;
        public Uri Artwork6
        {
            get { return artwork6; }
            set { SetProperty(ref artwork6, value); }
        }
        private Uri artwork7;
        public Uri Artwork7
        {
            get { return artwork7; }
            set { SetProperty(ref artwork7, value); }
        }

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

        public void LoadWithActual(HeroItemViewModel item)
        {
            LoadWithActual(item as GenericMusicItemViewModel);
            Artwork1 = item.Artwork1;
            Artwork2 = item.Artwork2;
            Artwork3 = item.Artwork3;
            Artwork4 = item.Artwork4;
            Artwork5 = item.Artwork5;
            Artwork6 = item.Artwork6;
            Artwork7 = item.Artwork7;

            RaisePropertyChanged("MainColor1");
        }
    }
}
