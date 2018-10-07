// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Music.Core.Storage;
using Aurora.Music.Core.Tools;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using Aurora.Shared.MVVM;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace Aurora.Music.ViewModels
{
    public class SongGroup : GroupedItem<SongViewModel>
    {
    }

    public class SongViewModel : ViewModelBase, IKey
    {
        public SongViewModel()
        {
        }

        private bool listMultiSelecting;
        public bool ListMultiSelecting
        {
            get { return listMultiSelecting; }
            set { SetProperty(ref listMultiSelecting, value); }
        }

        public override string ToString()
        {
            return $"{Title} - {string.Format(Consts.Localizer.GetString("TileDesc"), album, GetFormattedArtists())}, {GetAddtionalDesc()}";
        }

        private bool isOnline;
        public bool IsOnline
        {
            get { return isOnline; }
            set { SetProperty(ref isOnline, value); }
        }

        public bool IsPodcast { get; set; }
        public bool IsVideo { get; set; }

        private uint disc = 1;
        public uint Disc
        {
            get { return disc; }
            set { disc = value; }
        }


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
            if (IsOnedrive)
            {
                descs.Add("Not cache in local");
            }
            return string.Join(" · ", descs);
        }

        private string GetFileType()
        {
            if (IsPodcast)
            {
                return string.Format(Consts.Localizer.GetString("PodcastDesc"), song.PubDate.PubDatetoString($"'{Consts.Today}' H:mm", "ddd H:mm", "M/dd H:mm", "yy/M/dd", Consts.Next, Consts.Last));
            }
            if (IsOnline)
            {
                return Consts.Localizer.GetString("OnlineContentText");
            }
            var ext = FilePath.Split('.').Last().ToLower();
            switch (ext)
            {
                case "mp3": return "MPEG-Layer 3";
                case "flac": return "Free Lossless";
                case "m4a":
                    if (Song.BitRate > 400 * 1000)
                    {
                        return "Apple Lossless";
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

        private bool isOnedrive;
        public bool IsOnedrive
        {
            get { return isOnedrive; }
            set { SetProperty(ref isOnedrive, value); }
        }

        public SongViewModel(Song song)
        {
            Song = song;
            ID = song.ID;
            IsOnline = song.IsOnline;
            if (IsOnline)
            {
                FilePath = song.OnlineUri.AbsolutePath;
            }
            IsOnedrive = song.IsOnedrive;
            Rating = song.Rating;
            FilePath = song.FilePath;
            Album = song.Album;
            Title = song.Title;
            Track = song.Track;
            Artwork = song.PicturePath.IsNullorEmpty() ? null : new Uri(song.PicturePath);
            Duration = song.Duration;
            PubDate = song.PubDate;
            IsPodcast = song.IsPodcast;
            IsVideo = song.IsVideo;
            Disc = song.Disc;
        }

        private DateTime pubDate;
        public DateTime PubDate
        {
            get { return pubDate; }
            set { SetProperty(ref pubDate, value); }
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
                return Consts.Localizer.GetString("VariousText");
            }
            else
            {
                return string.Join(Consts.CommaSeparator, arr);
            }
        }

        public string DurationtoString(TimeSpan t1)
        {
            return $"{(int)(Math.Floor(t1.TotalMinutes))}{CultureInfoHelper.CurrentCulture.DateTimeFormat.TimeSeparator}{t1.Seconds.ToString("00")}";
        }

        public string PubDatetoString(DateTime d)
        {
            return d.PubDatetoString($"'{Consts.Today}'", "ddd", "M/dd ddd", "yy/M/dd", Consts.Next, Consts.Last);
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
            if (Song.Performers.IsNullorEmpty())
                return Song.AlbumArtists.IsNullorEmpty() ? (IsOnedrive ? "Not cache in local" : Consts.UnknownArtists) : string.Join(Consts.CommaSeparator, Song.AlbumArtists);
            else
            {
                return string.Join(Consts.CommaSeparator, Song.Performers);
            }
        }

        private string title;
        public string Title
        {
            get { return title.IsNullorEmpty() ? (FilePath ?? "Unknown").Split('\\').LastOrDefault() : title; }
            set { title = value; }
        }

        private string album;
        public string Album
        {
            get { return album.IsNullorEmpty() ? Consts.UnknownAlbum : album; }
            set { album = value; }
        }

        public string VideoIndicator(bool b)
        {
            return b ? "\uE173 " : "";
        }

        public DelegateCommand DownloadPodcast
        {
            get => new DelegateCommand(async () =>
            {
                if (!song.IsOnline) return;
                if (!CanDownload) return;
                var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Podcasts", CreationCollisionOption.OpenIfExists);
                var fileName = song.GetFileName();
                try
                {
                    CanDownload = false;
                    MainPage.Current.PopMessage("Start Caching");
                    var file = await Downloader.Current.StartDownload(new Uri(song.FilePath), fileName, folder, new DownloadDesc()
                    {
                        Title = song.Title,
                        Description = "Podcast",
                    });
                    IsOnline = false;
                    song.IsOnline = false;
                    song.FilePath = file.Path;
                }
                catch (Exception)
                {

                }
                finally
                {
                    CanDownload = true;
                }
            });
        }

        public string IsPodcastDownloadable(bool a)
        {
            return a ? "\uE118" : "\uE10B";
        }

        public bool And(bool a, bool s)
        {
            return a && s;
        }

        private bool fav;
        public bool Favorite
        {
            get { return fav; }
            set
            {
                SetProperty(ref fav, value);
                Song.WriteFavAsync(value);
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

        private bool canDownload = true;
        public bool CanDownload
        {
            get { return canDownload; }
            set { SetProperty(ref canDownload, value); }
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

        public string ShowOnline(bool a)
        {
            return a ? "\uE753" : string.Empty;
        }

        public string FormattedAlbum
        {
            get
            {
                return Album;
            }
        }

        public string Key
        {
            get
            {
                return Title;

            }
        }
    }
}
