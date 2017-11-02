using System;
using Aurora.Music.Core.Storage;
using Aurora.Shared.Extensions;
using Aurora.Music.Core.Utils;

namespace Aurora.Music.ViewModels
{
    class SongViewModel
    {
        public SongViewModel()
        {

        }

        public SongViewModel(SONG song)
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
            AlbumArtists = song.AlbumArtists.Split(new string[] { "$|$" }, StringSplitOptions.RemoveEmptyEntries);
            AlbumArtistsSort = song.AlbumArtistsSort.Split(new string[] { "$|$" }, StringSplitOptions.RemoveEmptyEntries);
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
            Composers = song.Composers.Split(new string[] { "$|$" }, StringSplitOptions.RemoveEmptyEntries);
            ComposersSort = song.ComposersSort.Split(new string[] { "$|$" }, StringSplitOptions.RemoveEmptyEntries);
            Conductor = song.Conductor;
            DiscCount = song.DiscCount;
            Copyright = song.Copyright;
            Genres = song.PerformersSort.Split(new string[] { "$|$" }, StringSplitOptions.RemoveEmptyEntries);
            Grouping = song.Grouping;
            Lyrics = song.Lyrics;
            Performers = song.Performers.Split(new string[] { "$|$" }, StringSplitOptions.RemoveEmptyEntries);
            PerformersSort = song.PerformersSort.Split(new string[] { "$|$" }, StringSplitOptions.RemoveEmptyEntries);
            Year = song.Year;
            PicturePath = song.PicturePath;
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
            if (Track == 0)
            {
                return (Index + 1).ToString();
            }
            return Track.ToString();
        }

        public int ID { get; set; }
        public uint Index { get; set; }
        public string FilePath { get; set; }
        public string PicturePath { get; set; }

        public TimeSpan Duration { get; set; }
        public uint BitRate { get; private set; }
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
        public string Title { get; set; }
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
        public uint Track { get; set; }
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
