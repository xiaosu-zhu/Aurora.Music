using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurora.Shared.Extensions;
using TagLib;
using Windows.Storage;

namespace Aurora.Music.Core.Models
{
    public class Song
    {
        public Song() { }

        public Song(Storage.Song song)
        {
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
            Genres = song.PerformersSort.Split(new string[] { "$|$" }, StringSplitOptions.RemoveEmptyEntries); ;
            Grouping = song.Grouping;
            Lyrics = song.Lyrics;
            Performers = song.Performers.Split(new string[] { "$|$" }, StringSplitOptions.RemoveEmptyEntries);
            PerformersSort = song.PerformersSort.Split(new string[] { "$|$" }, StringSplitOptions.RemoveEmptyEntries);
            Year = song.Year;
            PicturePath = song.PicturePath;
        }

        public static async Task<Song> Create(Tag tag, string path)
        {
            var s = new Song
            {
                FilePath = path,
                MusicBrainzArtistId = tag.MusicBrainzArtistId,
                MusicBrainzDiscId = tag.MusicBrainzDiscId,
                MusicBrainzReleaseArtistId = tag.MusicBrainzReleaseArtistId,
                MusicBrainzReleaseCountry = tag.MusicBrainzReleaseCountry,
                MusicBrainzReleaseId = tag.MusicBrainzReleaseId,
                MusicBrainzReleaseStatus = tag.MusicBrainzReleaseStatus,
                MusicBrainzReleaseType = tag.MusicBrainzReleaseType,
                MusicBrainzTrackId = tag.MusicBrainzTrackId,
                MusicIpId = tag.MusicIpId,
                BeatsPerMinute = tag.BeatsPerMinute,
                Album = tag.Album,
                AlbumArtists = tag.AlbumArtists,
                AlbumArtistsSort = tag.AlbumArtistsSort,
                AlbumSort = tag.AlbumSort,
                AmazonId = tag.AmazonId,
                Title = tag.Title,
                TitleSort = tag.TitleSort,
                Track = tag.Track,
                TrackCount = tag.TrackCount,
                ReplayGainTrackGain = tag.ReplayGainTrackGain,
                ReplayGainTrackPeak = tag.ReplayGainTrackPeak,
                ReplayGainAlbumGain = tag.ReplayGainAlbumGain,
                ReplayGainAlbumPeak = tag.ReplayGainAlbumPeak,
                Comment = tag.Comment,
                Disc = tag.Disc,
                Composers = tag.Composers,
                ComposersSort = tag.ComposersSort,
                Conductor = tag.Conductor,
                DiscCount = tag.DiscCount,
                Copyright = tag.Copyright,
                Genres = tag.Genres,
                Grouping = tag.Grouping,
                Lyrics = tag.Lyrics,
                Performers = tag.Performers,
                PerformersSort = tag.PerformersSort,
                Year = tag.Year,
                PicturePath = await GetPicturePath(tag.Pictures, tag.Album)
            };
            return s;
        }

        private async static Task<string> GetPicturePath(IPicture[] pictures, string album)
        {
            if (!pictures.IsNullorEmpty())
            {
                album = Utils.InvalidFileNameChars.Aggregate(album, (current, c) => current.Replace(c + "", "_"));
                var artworkFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Artworks", CreationCollisionOption.OpenIfExists);
                try
                {
                    var s = await artworkFolder.GetFileAsync(album);
                    if (s == null)
                    {
                        StorageFile cacheImg = await artworkFolder.CreateFileAsync(album, CreationCollisionOption.ReplaceExisting);
                        await FileIO.WriteBytesAsync(cacheImg, pictures[0].Data.Data);
                        return cacheImg.Path;
                    }
                    else
                    {
                        return s.Path;
                    }
                }
                catch (Exception)
                {
                    StorageFile cacheImg = await artworkFolder.CreateFileAsync(album, CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteBytesAsync(cacheImg, pictures[0].Data.Data);
                    return cacheImg.Path;
                }
            }
            else
            {
                return string.Empty;
            }
        }

        public string FilePath { get; set; }
        public string PicturePath { get; set; }

        public virtual string MusicBrainzReleaseId { get; set; }
        public virtual string MusicBrainzDiscId { get; set; }
        public virtual string MusicIpId { get; set; }
        public virtual string AmazonId { get; set; }
        public virtual string MusicBrainzReleaseStatus { get; set; }
        public virtual string MusicBrainzReleaseType { get; set; }
        public virtual string MusicBrainzReleaseCountry { get; set; }
        public virtual double ReplayGainTrackGain { get; set; }
        public virtual double ReplayGainTrackPeak { get; set; }
        public virtual double ReplayGainAlbumGain { get; set; }
        public virtual double ReplayGainAlbumPeak { get; set; }
        //public virtual IPicture[] Pictures { get; set; }
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
        public virtual string MusicBrainzTrackId { get; set; }
        public virtual string MusicBrainzReleaseArtistId { get; set; }
        public virtual bool IsEmpty { get; }
        public virtual string MusicBrainzArtistId { get; set; }
        public TagTypes TagTypes { get; }
        public virtual string Title { get; set; }
        public virtual string TitleSort { get; set; }
        public virtual string[] Performers { get; set; }
        public virtual string[] PerformersSort { get; set; }
        public virtual string[] AlbumArtists { get; set; }
        public virtual string[] AlbumArtistsSort { get; set; }
        public virtual string[] Composers { get; set; }
        public virtual string[] ComposersSort { get; set; }
        public virtual string Album { get; set; }
        public string JoinedGenres { get; }
        public virtual string AlbumSort { get; set; }
        public virtual string[] Genres { get; set; }
        public virtual uint Year { get; set; }
        public virtual uint Track { get; set; }
        public virtual uint TrackCount { get; set; }
        public virtual uint Disc { get; set; }
        public virtual uint DiscCount { get; set; }
        public virtual string Lyrics { get; set; }
        public virtual string Grouping { get; set; }
        public virtual uint BeatsPerMinute { get; set; }
        public virtual string Conductor { get; set; }
        public virtual string Copyright { get; set; }
        public virtual string Comment { get; set; }
    }


    public class Album
    {

        public int[] Songs { get; set; }

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
    }
}
