using System;
using Aurora.Music.Core.Storage;
using Aurora.Shared.Extensions;
using Aurora.Music.Core.Utils;
using Aurora.Music.Core;
using System.Linq;
using Aurora.Music.Core.Models;
using System.Collections.Generic;

namespace Aurora.Music.ViewModels
{
    class SongViewModel
    {
        public SongViewModel()
        {

        }

        public string GetAddtionalDesc()
        {
            var descs = new List<string>();
            if (Year != default(uint))
            {
                descs.Add(Year.ToString());
            }
            if (!Genres.IsNullorEmpty())
            {
                descs.AddRange(Genres);
            }
            if (BitRate != default(uint) && BitRate != 0u)
            {
                descs.Add($"{BitRate / 1024} Kbps");
            }
            if (SampleRate != default(uint))
            {
                descs.Add($"{SampleRate / 1000} KHz");
            }
            var type = GetFileType();
            if (!type.IsNullorEmpty())
                descs.Add(type);
            return string.Join(" · ", descs);
        }

        private string GetFileType()
        {
            var ext = FilePath.Split('.').Last().ToLower();
            switch (ext)
            {
                case "mp3": return "MPEG-Layer 3";
                case "flac": return "Free Loseless";
                case "m4a":
                    if (BitRate > 400 * 1024)
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

        public SongViewModel(Song song)
        {
            ID = song.ID;
            Duration = song.Duration;
            BitRate = song.BitRate;
            FilePath = song.FilePath;
            MusicBrainzArtistId = song.MusicBrainzArtistId;
            MusicBrainzDiscId = song.MusicBrainzDiscId;
            MusicBrainzReleaseArtistId = song.MusicBrainzReleaseArtistId;
            MusicBrainzReleaseCountry = song.MusicBrainzReleaseCountry;
            MusicBrainzReleaseId = song.MusicBrainzReleaseId;
            MusicBrainzReleaseStatus = song.MusicBrainzReleaseStatus;
            MusicBrainzReleaseType = song.MusicBrainzReleaseType;
            MusicBrainzTrackId = song.MusicBrainzTrackId;
            MusicIpId = song.MusicIpId;
            BeatsPerMinute = song.BeatsPerMinute;
            Album = song.Album;
            AlbumArtists = song.AlbumArtists;
            AlbumArtistsSort = song.AlbumArtistsSort;
            AlbumSort = song.AlbumSort;
            AmazonId = song.AmazonId;
            Title = song.Title;
            TitleSort = song.TitleSort;
            Track = song.Track;
            TrackCount = song.TrackCount;
            ReplayGainTrackGain = song.ReplayGainTrackGain;
            ReplayGainTrackPeak = song.ReplayGainTrackPeak;
            ReplayGainAlbumGain = song.ReplayGainAlbumGain;
            ReplayGainAlbumPeak = song.ReplayGainAlbumPeak;
            Comment = song.Comment;
            Disc = song.Disc;
            Composers = song.Composers;
            ComposersSort = song.ComposersSort;
            Conductor = song.Conductor;
            DiscCount = song.DiscCount;
            Copyright = song.Copyright;
            Genres = song.PerformersSort;
            Grouping = song.Grouping;
            Lyrics = song.Lyrics;
            Performers = song.Performers;
            PerformersSort = song.PerformersSort;
            Year = song.Year;
            PicturePath = song.PicturePath;
            SampleRate = song.SampleRate;
            AudioChannels = song.AudioChannels;
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

        public string DurationtoString(TimeSpan t)
        {
            return t.ToString(@"m\:ss");
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

        private string picturePath;
        public string PicturePath
        {
            get { return picturePath.IsNullorEmpty() ? Consts.BlackPlaceholder : picturePath; }
            set { picturePath = value; }
        }

        public TimeSpan Duration { get; set; }
        public uint BitRate { get; private set; }


        public int SampleRate { get; private set; }
        public int AudioChannels { get; private set; }

        public string MusicBrainzReleaseId { get; set; }
        public string MusicBrainzDiscId { get; set; }
        public string MusicIpId { get; set; }
        public string AmazonId { get; set; }
        public string MusicBrainzReleaseStatus { get; set; }
        public string MusicBrainzReleaseType { get; set; }
        public string MusicBrainzReleaseCountry { get; set; }
        public double ReplayGainTrackGain { get; set; }
        public double ReplayGainTrackPeak { get; set; }
        public double ReplayGainAlbumGain { get; set; }
        public double ReplayGainAlbumPeak { get; set; }
        //public IPicture[] Pictures { get; set; }
        public string FirstAlbumArtist { get; }
        public string FirstAlbumArtistSort { get; }
        public string FirstPerformer { get; }
        public string FirstPerformerSort { get; }
        public string FirstComposerSort { get; }
        public string FirstComposer { get; }
        public string FirstGenre { get; }
        public string JoinedAlbumArtists { get; }
        public string JoinedPerformers { get; }
        public string JoinedPerformersSort { get; }
        public string JoinedComposers { get; }
        public string MusicBrainzTrackId { get; set; }
        public string MusicBrainzReleaseArtistId { get; set; }
        public bool IsEmpty { get; }
        public string MusicBrainzArtistId { get; set; }

        private string title;
        public string Title
        {
            get { return title.IsNullorEmpty() ? FilePath.Split('\\').LastOrDefault() : title; }
            set { title = value; }
        }

        public string TitleSort { get; set; }
        public string[] Performers { get; set; }
        public string[] PerformersSort { get; set; }
        public string[] AlbumArtists { get; set; }
        public string[] AlbumArtistsSort { get; set; }
        public string[] Composers { get; set; }
        public string[] ComposersSort { get; set; }
        public string Album { get; set; }
        public string JoinedGenres { get; }
        public string AlbumSort { get; set; }
        public string[] Genres { get; set; }
        public uint Year { get; set; }

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


        public uint TrackCount { get; set; }
        public uint Disc { get; set; }
        public uint DiscCount { get; set; }
        public string Lyrics { get; set; }
        public string Grouping { get; set; }
        public uint BeatsPerMinute { get; set; }
        public string Conductor { get; set; }
        public string Copyright { get; set; }
        public string Comment { get; set; }
    }
}
