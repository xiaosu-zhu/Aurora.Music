using System;
using System.Linq;
using System.Threading.Tasks;
using Aurora.Shared.Extensions;
using TagLib;
using Windows.Storage;
using System.IO;
using Windows.Storage.FileProperties;
using Aurora.Shared.Helpers;
using System.Collections.Generic;
using Aurora.Music.Core.Storage;
using Windows.UI;

namespace Aurora.Music.Core.Models
{

    public enum MediaType
    {
        Song, Album, PlayList, Artist
    }

    public class GenericMusicItem
    {
        public int ContextualID { get; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Addtional { get; set; }

        public bool IsOnline { get; set; }

        public string PicturePath { get; set; }

        public int[] IDs { get; set; }
        public MediaType InnerType { get; set; }

        internal GenericMusicItem(SONG s)
        {
            InnerType = MediaType.Song;
            ContextualID = s.ID;
            Title = s.Title;
            Description = s.Album;
            Addtional = s.AlbumArtists.IsNullorEmpty() ? "Unknown Artists" : string.Join(", ", s.AlbumArtists.Split(new string[] { "$|$" }, StringSplitOptions.RemoveEmptyEntries));
            IDs = new int[] { s.ID };
            PicturePath = s.PicturePath;
        }
        internal GenericMusicItem(ALBUM s)
        {
            InnerType = MediaType.Album;
            ContextualID = s.ID;
            Title = s.Name;
            var songs = s.Songs.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            var ids = Array.ConvertAll(songs, (a) =>
            {
                return int.Parse(a);
            });
            Description = ids.Length.ToString() + (ids.Length == 1 ? " Song" : " Songs");
            Addtional = s.AlbumArtists.IsNullorEmpty() ? "Unknown Artists" : string.Join(", ", s.AlbumArtists.Split(new string[] { "$|$" }, StringSplitOptions.RemoveEmptyEntries));

            var songIDs = AsyncHelper.RunSync(async () => await SQLOperator.Current().GetSongsAsync(ids));
            var s1 = songIDs.OrderBy(a => a.Track);
            s1 = s1.OrderBy(a => a.Disc);
            IDs = s1.Select(b => b.ID).ToArray();

            PicturePath = s.PicturePath;
        }

        public GenericMusicItem() { }


        internal GenericMusicItem(Album s)
        {
            InnerType = MediaType.Album;
            ContextualID = s.ID;
            Title = s.Name;
            var ids = s.Songs;
            Description = ids.Length.ToString() + (ids.Length == 1 ? " Song" : " Songs");
            Addtional = s.AlbumArtists.IsNullorEmpty() ? "Unknown Artists" : string.Join(", ", s.AlbumArtists);

            var songIDs = AsyncHelper.RunSync(async () => await SQLOperator.Current().GetSongsAsync(ids));
            var s1 = songIDs.OrderBy(a => a.Track);
            s1 = s1.OrderBy(a => a.Disc);
            IDs = s1.Select(b => b.ID).ToArray();

            PicturePath = s.PicturePath;
        }
    }

    public class ListWithKey<T> : List<T>
    {
        public string Key { get; }

        public ListWithKey(string key, IEnumerable<T> items)
        {
            Key = key;
            AddRange(items);
        }
    }

    public class Song
    {
        public Song() { }

        internal Song(Storage.SONG song)
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
            SampleRate = song.SampleRate;
            AudioChannels = song.AudioChannels;
        }

        public static async Task<Song> Create(Tag tag, string path, MusicProperties music)
        {
            var s = new Song
            {
                Duration = music.Duration,
                BitRate = music.Bitrate,
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
                Year = tag.Year
            };

            s.PicturePath = await s.GetPicturePath(tag.Pictures, tag.Album);
            return s;
        }

        private async Task<string> GetPicturePath(IPicture[] pictures, string album)
        {
            if (!pictures.IsNullorEmpty())
            {
                album = Shared.Utils.InvalidFileNameChars.Aggregate(album, (current, c) => current.Replace(c + "", "_"));
                album = $"{album}.{pictures[0].MimeType.Split('/').LastOrDefault().Replace("jpeg", "jpg")}";
                try
                {
                    var s = await Consts.ArtworkFolder.GetFileAsync(album);
                    if (s == null)
                    {
                        StorageFile cacheImg = await Consts.ArtworkFolder.CreateFileAsync(album, CreationCollisionOption.ReplaceExisting);
                        await FileIO.WriteBytesAsync(cacheImg, pictures[0].Data.Data);
                        return cacheImg.Path;
                    }
                    else
                    {
                        return s.Path;
                    }
                }
                catch (FileNotFoundException)
                {
                    StorageFile cacheImg = await Consts.ArtworkFolder.CreateFileAsync(album, CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteBytesAsync(cacheImg, pictures[0].Data.Data);
                    return cacheImg.Path;
                }
            }
            else
            {
                return string.Empty;
            }
        }

        public TimeSpan Duration { get; set; }
        public uint BitRate { get; set; }

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
        public string FirstAlbumArtist { get; set; }
        public string FirstAlbumArtistSort { get; set; }
        public string FirstPerformer { get; set; }
        public string FirstPerformerSort { get; set; }
        public string FirstComposerSort { get; set; }
        public string FirstComposer { get; set; }
        public string FirstGenre { get; set; }
        public string JoinedAlbumArtists { get; set; }
        public string JoinedPerformers { get; set; }
        public string JoinedPerformersSort { get; set; }
        public string JoinedComposers { get; set; }
        public virtual string MusicBrainzTrackId { get; set; }
        public virtual string MusicBrainzReleaseArtistId { get; set; }
        public virtual bool IsEmpty { get; set; }
        public virtual string MusicBrainzArtistId { get; set; }
        public TagTypes TagTypes { get; set; }
        public virtual string Title { get; set; }
        public virtual string TitleSort { get; set; }
        public virtual string[] Performers { get; set; }
        public virtual string[] PerformersSort { get; set; }
        public virtual string[] AlbumArtists { get; set; }
        public virtual string[] AlbumArtistsSort { get; set; }
        public virtual string[] Composers { get; set; }
        public virtual string[] ComposersSort { get; set; }
        public virtual string Album { get; set; }
        public string JoinedGenres { get; set; }
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
        public int ID { get; set; }
        public int SampleRate { get; private set; }
        public int AudioChannels { get; private set; }
    }


    public class Album
    {

        internal Album(Storage.ALBUM album)
        {
            ID = album.ID;
            var songs = album.Songs.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            Songs = Array.ConvertAll(songs, (a) =>
            {
                return int.Parse(a);
            });
            Name = album.Name;
            Genres = album.Genres.IsNullorEmpty() ? null : album.Genres.Split(new string[] { "$|$" }, StringSplitOptions.RemoveEmptyEntries);
            Year = album.Year;
            AlbumSort = album.AlbumSort;
            TrackCount = album.TrackCount;
            DiscCount = album.DiscCount;
            AlbumArtists = album.AlbumArtists.IsNullorEmpty() ? null : album.AlbumArtists.Split(new string[] { "$|$" }, StringSplitOptions.RemoveEmptyEntries);
            AlbumArtistsSort = album.AlbumArtistsSort.IsNullorEmpty() ? null : album.AlbumArtistsSort.Split(new string[] { "$|$" }, StringSplitOptions.RemoveEmptyEntries);
            ReplayGainAlbumGain = album.ReplayGainAlbumGain;
            ReplayGainAlbumPeak = album.ReplayGainAlbumPeak;
            PicturePath = album.PicturePath;
        }

        internal Album(IGrouping<string, SONG> album)
        {
            Name = album.Key.IsNullorEmpty() ? "Unknown Album" : album.Key;

            // uint value, use their max value
            DiscCount = album.Max(x => x.DiscCount);
            TrackCount = album.Max(x => x.TrackCount);
            Year = album.Max(x => x.Year);

            // TODO: not combine all, just use not-null value
            // string[] value, use their all value (remove duplicated values) combine
            AlbumArtists = (from aa in album where !aa.AlbumArtists.IsNullorEmpty() select aa.AlbumArtists).FirstOrDefault()?.Split(new string[] { "$|$" }, StringSplitOptions.RemoveEmptyEntries);//album.Where(x => !x.AlbumArtists.IsNullorEmpty()).FirstOrDefault().AlbumArtists;
            Genres = (from aa in album where !aa.Genres.IsNullorEmpty() select aa.Genres).FirstOrDefault()?.Split(new string[] { "$|$" }, StringSplitOptions.RemoveEmptyEntries);
            AlbumArtistsSort = (from aa in album where !aa.AlbumArtistsSort.IsNullorEmpty() select aa.AlbumArtistsSort).FirstOrDefault()?.Split(new string[] { "$|$" }, StringSplitOptions.RemoveEmptyEntries);

            // normal value, use their not-null value
            AlbumSort = (from aa in album where !aa.AlbumSort.IsNullorEmpty() select aa.AlbumSort).FirstOrDefault();
            ReplayGainAlbumGain = (from aa in album where aa.ReplayGainAlbumGain != double.NaN select aa.ReplayGainAlbumGain).FirstOrDefault();
            ReplayGainAlbumPeak = (from aa in album where aa.ReplayGainAlbumPeak != double.NaN select aa.ReplayGainAlbumPeak).FirstOrDefault();
            PicturePath = (from aa in album where !aa.PicturePath.IsNullorEmpty() select aa.PicturePath).FirstOrDefault();

            // songs, serialized as "ID0|ID1|ID2...|IDn"
            Songs = album.Select(x => x.ID).Distinct().ToArray();
        }

        public int[] Songs { get; set; }

        public string PicturePath { get; set; }

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
        public int ID { get; }
    }
}
