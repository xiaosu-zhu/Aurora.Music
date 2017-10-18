using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurora.Shared.Extensions;
using SQLite;
using Windows.Storage;
using TagLib;
using Aurora.Music.Core.Models;

namespace Aurora.Music.Core.Storage
{
    public class Song
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        public Song() { }


        public Song(Models.Song song)
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
            AlbumArtists = string.Join("$|$", song.AlbumArtists);
            AlbumArtistsSort = string.Join("$|$", song.AlbumArtistsSort);
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
            Composers = string.Join("$|$", song.Composers);
            ComposersSort = string.Join("$|$", song.ComposersSort);
            Conductor = song.Conductor;
            DiscCount = song.DiscCount;
            Copyright = song.Copyright;
            Genres = string.Join("$|$", song.Genres);
            Grouping = song.Grouping;
            Lyrics = song.Lyrics;
            Performers = string.Join("$|$", song.Performers);
            PerformersSort = string.Join("$|$", song.PerformersSort);
            Year = song.Year;
            PicturePath = song.PicturePath;
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
        public virtual string Performers { get; set; }
        public virtual string PerformersSort { get; set; }
        public virtual string AlbumArtists { get; set; }
        public virtual string AlbumArtistsSort { get; set; }
        public virtual string Composers { get; set; }
        public virtual string ComposersSort { get; set; }
        public virtual string Album { get; set; }
        public string JoinedGenres { get; set; }
        public virtual string AlbumSort { get; set; }
        public virtual string Genres { get; set; }
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
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        public Album() { }

        public Album(Models.Album album)
        {
            Songs = string.Join('|', album.Songs);
            Name = album.Name;
            Genres = string.Join("$|$", album.Genres);
            Year = album.Year;
            AlbumSort = album.AlbumSort;
            TrackCount = album.TrackCount;
            DiscCount = album.DiscCount;
            AlbumArtists = string.Join("$|$", album.AlbumArtists);
            AlbumArtistsSort = string.Join("$|$", album.AlbumArtistsSort);
            ReplayGainAlbumGain = album.ReplayGainAlbumGain;
            ReplayGainAlbumPeak = album.ReplayGainAlbumPeak;
            PicturePath = album.PicturePath;
        }

        public string Songs { get; set; }

        public string PicturePath { get; set; }

        [Unique]
        public string Name { get; set; }
        public virtual string Genres { get; set; }
        public virtual uint Year { get; set; }
        public virtual string AlbumSort { get; set; }
        public virtual uint TrackCount { get; set; }
        public virtual uint DiscCount { get; set; }
        public virtual string AlbumArtists { get; set; }
        public virtual string AlbumArtistsSort { get; set; }
        public virtual double ReplayGainAlbumGain { get; set; }
        public virtual double ReplayGainAlbumPeak { get; set; }
    }

    public class SQLOperator : IDisposable
    {
        private static readonly string DB_PATH = "main.db";
        private SQLiteAsyncConnection conn;

        public event EventHandler<SongsAddedEventArgs> NewSongsAdded;

        /// <summary>
        /// Including album added and multiple album changed.
        /// </summary>
        public event EventHandler<AlbumModifiedEventArgs> AlbumModified;

        public SQLOperator()
        {
            conn = new SQLiteAsyncConnection(DB_PATH);
        }

        public async Task CreateTable()
        {
            await conn.CreateTableAsync<Song>();
            await conn.CreateTableAsync<Album>();
        }

        public async Task<bool> UpdateAsync(Models.Song song)
        {
            var tag = new Song(song);

            var result = await conn.QueryAsync<Song>("SELECT ID FROM SONG WHERE FILEPATH = ?", song.FilePath);
            if (result.Count > 0)
            {
                return true;
            }
            else
            {
                await conn.InsertAsync(tag);
                return true;
            }
        }

        internal async Task UpdateSongListAsync(List<Song> tempList)
        {
            var newlist = new List<Song>();
            foreach (var item in tempList)
            {
                var result = await conn.QueryAsync<Song>("SELECT ID FROM SONG WHERE FILEPATH = ?", item.FilePath);
                if (result.Count > 0)
                {
                    continue;
                }
                else
                {
                    await conn.InsertAsync(item);
                    newlist.Add(item);
                }
            }
            if (newlist.Count > 0)
            {
                NewSongsAdded?.Invoke(this, new SongsAddedEventArgs(newlist.ToArray()));
            }
        }

        private static SQLOperator current;

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (conn != null)
                    {
                        conn.GetConnection().Dispose();
                    }
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~SQLOperator() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }

        public static async Task<SQLOperator> CurrentAsync()
        {
            if (current != null && !current.disposedValue)
            {
                return current;
            }
            else if (current != null)
            {
                current.Dispose();
                current = null;
            }

            var p = new SQLOperator();
            await p.CreateTable();
            current = p;
            return p;
        }

        internal async Task<List<T>> GetAllAsync<T>() where T : new()
        {
            return await conn.Table<T>().ToListAsync();
        }

        internal async Task AddtoAlbumAsync(IEnumerable<IGrouping<string, Song>> albums)
        {
            var newlist = new List<Album>();
            foreach (var album in albums)
            {
                var result = await conn.QueryAsync<Album>("SELECT * FROM ALBUM WHERE NAME = ?", album.Key);
                if (result.Count > 0)
                {
                    var p = result[0];
                    p.Songs = p.Songs + '|' + string.Join('|', album.Select(x => x.ID).Distinct());
                    if (p.AlbumArtists.IsNullorEmpty())
                    {
                        p.AlbumArtists = string.Join("$|$", album.Select(x => x.AlbumArtists));
                    }
                    if (p.AlbumArtistsSort.IsNullorEmpty())
                    {
                        p.AlbumArtistsSort = string.Join("$|$", album.Select(x => x.AlbumArtistsSort));
                    }
                    if (p.PicturePath.IsNullorEmpty())
                    {
                        p.PicturePath = album.Where(x => x.PicturePath.IsNullorEmpty()).FirstOrDefault().PicturePath;
                    }
                    if (p.Year == default(int))
                    {
                        p.Year = album.Where(x => x.Year != 0).FirstOrDefault().Year;
                    }
                    if (p.Genres.IsNullorEmpty())
                    {
                        p.Genres = string.Join("$|$", album.Select(x => x.Genres));
                    }
                    await conn.UpdateAsync(p);
                    newlist.Add(p);
                }
                else
                {
                    var a = new Album
                    {
                        Name = album.Key,
                        DiscCount = album.OrderByDescending(x => x.DiscCount).FirstOrDefault().DiscCount,
                        TrackCount = album.OrderByDescending(x => x.DiscCount).FirstOrDefault().Track,

                        AlbumArtists = string.Join("$|$", album.Select(x => x.AlbumArtists)),
                        Genres = string.Join("$|$", album.Select(x => x.Genres)),
                        AlbumArtistsSort = string.Join("$|$", album.Select(x => x.AlbumArtistsSort)),

                        AlbumSort = album.Where(x => !x.AlbumSort.IsNullorEmpty()).FirstOrDefault().AlbumSort,
                        ReplayGainAlbumGain = album.Where(x => x.ReplayGainAlbumGain != 0).FirstOrDefault().ReplayGainAlbumGain,
                        ReplayGainAlbumPeak = album.Where(x => x.ReplayGainAlbumPeak != 0).FirstOrDefault().ReplayGainAlbumPeak,
                        PicturePath = album.Where(x => x.PicturePath.IsNullorEmpty()).FirstOrDefault().PicturePath,
                        Year = album.Where(x => x.Year != 0).FirstOrDefault().Year,

                        Songs = string.Join('|', album.Select(x => x.ID).Distinct())
                    };
                    await conn.InsertAsync(a);
                    newlist.Add(a);
                }
            }
            if (newlist.Count > 0)
            {
                AlbumModified?.Invoke(this, new AlbumModifiedEventArgs(newlist.ToArray()));
            }
        }
    }
    #endregion
}
