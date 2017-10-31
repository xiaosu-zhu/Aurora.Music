using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurora.Shared.MVVM;
using Windows.UI.Xaml.Media.Imaging;
using Aurora.Music.Core.Utils;

namespace Aurora.Music.ViewModels
{
    class SongViewModel
    {
        public string DurationtoString(TimeSpan t)
        {
            return t.ToString(@"m\:ss");
        }

        public int ID { get; set; }
        public uint Index { get; set; }
        public string FilePath { get; set; }
        public string PicturePath { get; set; }

        public TimeSpan Duration { get; set; }
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
