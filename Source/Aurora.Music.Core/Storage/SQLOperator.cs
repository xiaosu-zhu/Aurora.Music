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
using Aurora.Shared.Helpers;
using Windows.UI;

namespace Aurora.Music.Core.Storage
{
    class STATISTICS
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        public int TargetID { get; set; }

        public int PlayedCount { get; set; }
        public bool Favorite { get; set; }

        /// <summary>
        /// 0: song, 1: album, 2: playlist
        /// </summary>
        public int TargetType { get; set; }

        public DateTime LastPlay { get; set; }
    }

    class PLAYSTATISTIC
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        public int TargetID { get; set; }
        /// <summary>
        /// 0: song, 1: album, 2: playlist
        /// </summary>
        public int TargetType { get; set; }

        public int Sunday { get; set; }
        public int Monday { get; set; }
        public int Tuesday { get; set; }
        public int Wednesday { get; set; }
        public int Thursday { get; set; }
        public int Friday { get; set; }
        public int Saturday { get; set; }

        public int Morning { get; set; }
        public int Noon { get; set; }
        public int Afternoon { get; set; }
        public int Evening { get; set; }
        public int Dusk { get; set; }

        public static readonly string[] dateProjection = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
        public static readonly string[] timeProjection = { "Morning", "Noon", "Afternoon", "Evening" };
    }

    class SONG : IEqualityComparer<SONG>
    {
        private string albumArtists;

        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        public SONG() { }


        public SONG(Song song)
        {
            FilePath = song.FilePath;
            Duration = song.Duration;
            BitRate = song.BitRate;
            SampleRate = song.SampleRate;
            AudioChannels = song.AudioChannels;
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
            Performers = string.Join("$|$", song.Performers);
            PerformersSort = string.Join("$|$", song.PerformersSort);
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
            Year = song.Year;
            PicturePath = song.PicturePath;
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

        public int SampleRate { get; private set; }
        public int AudioChannels { get; private set; }

        private string title;

        public string Title
        {
            get => title.IsNullorEmpty() ? FilePath.Split('\\').LastOrDefault() : title;
            set { title = value; }
        }


        public virtual string TitleSort { get; set; }
        public virtual string Performers { get; set; }
        public virtual string PerformersSort { get; set; }
        public virtual string AlbumArtists
        {
            get
            {
                return albumArtists;
            }
            set
            {
                if (value.IsNullorEmpty())
                {
                    albumArtists = Performers;
                }
                else
                {
                    albumArtists = value;
                }
            }
        }
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

        public bool Equals(SONG x, SONG y)
        {
            return x.ID == y.ID;
        }

        public int GetHashCode(SONG obj)
        {
            return obj.ID.GetHashCode();
        }
    }


    class ALBUM
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        public ALBUM() { }

        public ALBUM(Album album)
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

    public class FOLDER
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        public int SongsCount { get; set; }
        public string Token { get; set; }

        [Unique]
        public string Path { get; set; }

        public FOLDER() { }

        public FOLDER(StorageFolder f)
        {
            // Application now has read/write access to all contents in the picked folder
            // (including other sub-folder contents)
            Token = Windows.Storage.AccessCache.StorageApplicationPermissions.
            FutureAccessList.Add(f);
            SongsCount = -1;
            Path = f.Path;
        }

        public async Task<StorageFolder> GetFolderAsync()
        {
            try
            {
                var f = await Windows.Storage.AccessCache.StorageApplicationPermissions.
                FutureAccessList.GetFolderAsync(Token);
                if (f.Path != Path)
                {
                    Path = f.Path;
                    await SQLOperator.Current().UpdateFolderAsync(this);
                }
                return f;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public class SQLOperator : IDisposable
    {

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
        #endregion
        private static readonly string DB_PATH = "main.db";
        private SQLiteAsyncConnection conn;

        /// <summary>
        /// use to provide persistent instance
        /// </summary>
        private static SQLOperator current;

        /// <summary>
        /// use to lock <see cref="Current"/> method.
        /// </summary>
        private static object lockable = new object();

        internal event EventHandler<SongsAddedEventArgs> NewSongsAdded;

        /// <summary>
        /// Including album added and multiple album changed.
        /// </summary>
        internal event EventHandler<AlbumModifiedEventArgs> AlbumModified;

        public static string SQLEscaping(string value)
        {
            return value.Replace("'", @"''");
        }

        private SQLOperator()
        {
            conn = new SQLiteAsyncConnection(DB_PATH);
            CreateTable();
        }

        private void CreateTable()
        {
            conn.GetConnection().CreateTable<SONG>();
            conn.GetConnection().CreateTable<ALBUM>();
            conn.GetConnection().CreateTable<FOLDER>();
            conn.GetConnection().CreateTable<STATISTICS>();
            conn.GetConnection().CreateTable<PLAYSTATISTIC>();
        }

        public async Task<bool> AddFolderAsync(StorageFolder folder)
        {
            var token = await conn.QueryAsync<FOLDER>("SELECT * FROM FOLDER WHERE PATH=?", folder.Path);
            if (token.Count > 0)
            {
                return false;
            }
            else
            {
                var f = new FOLDER(folder);
                await conn.InsertAsync(f);
                return true;
            }
        }

        public async Task UpdateFolderAsync(StorageFolder folder, int v)
        {
            var result = await conn.QueryAsync<FOLDER>("SELECT * FROM FOLDER WHERE PATH=?", folder.Path);
            if (result.Count > 0)
            {
                result[0].SongsCount = v;
                await conn.UpdateAsync(result[0]);
            }
            else
            {
                var f = new FOLDER(folder);
                await conn.InsertAsync(f);
            }
        }

        internal async Task<SONG> UpdateSongAsync(Song song)
        {
            var tag = new SONG(song);

            var result = await conn.QueryAsync<int>("SELECT ID FROM SONG WHERE FILEPATH = ?", song.FilePath);
            if (result.Count > 0)
            {
                return null;
            }
            else
            {
                await conn.InsertAsync(tag);
                return tag;
            }
        }

        public async Task UpdateSongListAsync(List<Song> tempList)
        {
            var newlist = new List<SONG>();
            foreach (var item in tempList)
            {
                var result = await conn.QueryAsync<SONG>("SELECT ID FROM SONG WHERE FILEPATH = ?", item.FilePath);
                if (result.Count > 0)
                {
                    continue;
                }
                else
                {
                    var song = new SONG(item);
                    await conn.InsertAsync(song);
                    newlist.Add(song);
                }
            }
            if (newlist.Count > 0)
            {
                NewSongsAdded?.Invoke(this, new SongsAddedEventArgs(newlist.ToArray().Select(x => new Song(x)).ToArray()));
            }
        }

        public static SQLOperator Current()
        {
            lock (lockable)
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
                current = p;
                return p;
            }
        }

        public async Task<List<T>> GetAllAsync<T>() where T : new()
        {
            return await conn.Table<T>().ToListAsync();
        }

        internal async Task AddAlbumAsync(IGrouping<string, Song> album)
        {
            var result = await conn.QueryAsync<ALBUM>("SELECT * FROM ALBUM WHERE NAME = ?", album.Key);
            if (result.Count > 0)
            {
                var p = result[0];

                // the properties' converting rules is described *below*

                p.Songs = p.Songs + '|' + string.Join('|', album.Select(x => x.ID).Distinct());
                if (p.AlbumArtists.IsNullorEmpty())
                {
                    p.AlbumArtists = string.Join("$|$", (from aa in album where !aa.AlbumArtists.IsNullorEmpty() select aa.AlbumArtists).FirstOrDefault());
                }
                if (p.AlbumArtistsSort.IsNullorEmpty())
                {
                    p.AlbumArtistsSort = string.Join("$|$", (from aa in album where !aa.AlbumArtistsSort.IsNullorEmpty() select aa.AlbumArtistsSort).FirstOrDefault());
                }
                if (p.Genres.IsNullorEmpty())
                {
                    p.Genres = string.Join("$|$", (from aa in album where !aa.Genres.IsNullorEmpty() select aa.Genres).FirstOrDefault());
                }
                if (p.PicturePath.IsNullorEmpty())
                {
                    p.PicturePath = (from aa in album where !aa.PicturePath.IsNullorEmpty() select aa.PicturePath).FirstOrDefault();
                }
                if (p.Year == default(uint))
                {
                    p.Year = album.Max(x => x.Year);
                }
                if (p.DiscCount == default(uint))
                {
                    p.DiscCount = album.Max(x => x.DiscCount);
                }
                if (p.TrackCount == default(uint))
                {
                    p.TrackCount = album.Max(x => x.TrackCount);
                }
                await conn.UpdateAsync(p);
            }
            else
            {
                var a = new ALBUM
                {
                    Name = album.Key,

                    // uint value, use their max value
                    DiscCount = album.Max(x => x.DiscCount),
                    TrackCount = album.Max(x => x.TrackCount),
                    Year = album.Max(x => x.Year),

                    // TODO: not combine all, just use not-null value
                    // string[] value, use their all value (remove duplicated values) combine
                    AlbumArtists = string.Join("$|$", (from aa in album where !aa.AlbumArtists.IsNullorEmpty() select aa.AlbumArtists).FirstOrDefault()),//album.Where(x => !x.AlbumArtists.IsNullorEmpty()).FirstOrDefault().AlbumArtists;
                    Genres = string.Join("$|$", (from aa in album where !aa.Genres.IsNullorEmpty() select aa.Genres).FirstOrDefault()),
                    AlbumArtistsSort = string.Join("$|$", (from aa in album where !aa.AlbumArtistsSort.IsNullorEmpty() select aa.AlbumArtistsSort).FirstOrDefault()),

                    // normal value, use their not-null value
                    AlbumSort = (from aa in album where !aa.AlbumSort.IsNullorEmpty() select aa.AlbumSort).FirstOrDefault(),
                    ReplayGainAlbumGain = (from aa in album where aa.ReplayGainAlbumGain != double.NaN select aa.ReplayGainAlbumGain).FirstOrDefault(),
                    ReplayGainAlbumPeak = (from aa in album where aa.ReplayGainAlbumPeak != double.NaN select aa.ReplayGainAlbumPeak).FirstOrDefault(),
                    PicturePath = (from aa in album where !aa.PicturePath.IsNullorEmpty() select aa.PicturePath).FirstOrDefault(),

                    // songs, serialized as "ID0|ID1|ID2...|IDn"
                    Songs = string.Join('|', album.Select(x => x.ID).Distinct())
                };
                await conn.InsertAsync(a);
            }
        }

        internal async Task AddAlbumAsync(IGrouping<string, SONG> album)
        {
            var result = await conn.QueryAsync<ALBUM>("SELECT * FROM ALBUM WHERE NAME = ?", album.Key);
            if (result.Count > 0)
            {
                var p = result[0];

                // the properties' converting rules is described *below*

                p.Songs = p.Songs + '|' + string.Join('|', album.Select(x => x.ID).Distinct());
                if (p.AlbumArtists.IsNullorEmpty())
                {
                    p.AlbumArtists = (from aa in album where !aa.AlbumArtists.IsNullorEmpty() select aa.AlbumArtists).FirstOrDefault();
                }
                if (p.AlbumArtistsSort.IsNullorEmpty())
                {
                    p.AlbumArtistsSort = (from aa in album where !aa.AlbumArtistsSort.IsNullorEmpty() select aa.AlbumArtistsSort).FirstOrDefault();
                }
                if (p.Genres.IsNullorEmpty())
                {
                    p.Genres = (from aa in album where !aa.Genres.IsNullorEmpty() select aa.Genres).FirstOrDefault();
                }
                if (p.PicturePath.IsNullorEmpty())
                {
                    p.PicturePath = (from aa in album where !aa.PicturePath.IsNullorEmpty() select aa.PicturePath).FirstOrDefault();
                }
                if (p.Year == default(uint))
                {
                    p.Year = album.Max(x => x.Year);
                }
                if (p.DiscCount == default(uint))
                {
                    p.DiscCount = album.Max(x => x.DiscCount);
                }
                if (p.TrackCount == default(uint))
                {
                    p.TrackCount = album.Max(x => x.TrackCount);
                }
                await conn.UpdateAsync(p);
            }
            else
            {
                var a = new ALBUM
                {
                    Name = album.Key,

                    // uint value, use their max value
                    DiscCount = album.Max(x => x.DiscCount),
                    TrackCount = album.Max(x => x.TrackCount),
                    Year = album.Max(x => x.Year),

                    // TODO: not combine all, just use not-null value
                    // string[] value, use their all value (remove duplicated values) combine
                    AlbumArtists = (from aa in album where !aa.AlbumArtists.IsNullorEmpty() select aa.AlbumArtists).FirstOrDefault(),//album.Where(x => !x.AlbumArtists.IsNullorEmpty()).FirstOrDefault().AlbumArtists;
                    Genres = (from aa in album where !aa.Genres.IsNullorEmpty() select aa.Genres).FirstOrDefault(),
                    AlbumArtistsSort = (from aa in album where !aa.AlbumArtistsSort.IsNullorEmpty() select aa.AlbumArtistsSort).FirstOrDefault(),

                    // normal value, use their not-null value
                    AlbumSort = (from aa in album where !aa.AlbumSort.IsNullorEmpty() select aa.AlbumSort).FirstOrDefault(),
                    ReplayGainAlbumGain = (from aa in album where aa.ReplayGainAlbumGain != double.NaN select aa.ReplayGainAlbumGain).FirstOrDefault(),
                    ReplayGainAlbumPeak = (from aa in album where aa.ReplayGainAlbumPeak != double.NaN select aa.ReplayGainAlbumPeak).FirstOrDefault(),
                    PicturePath = (from aa in album where !aa.PicturePath.IsNullorEmpty() select aa.PicturePath).FirstOrDefault(),

                    // songs, serialized as "ID0|ID1|ID2...|IDn"
                    Songs = string.Join('|', album.Select(x => x.ID).Distinct())
                };
                await conn.InsertAsync(a);
            }
        }

        internal async Task AddAlbumListAsync(IEnumerable<IGrouping<string, SONG>> albums)
        {
            var newlist = new List<ALBUM>();
            foreach (var album in albums)
            {
                var result = await conn.QueryAsync<ALBUM>("SELECT * FROM ALBUM WHERE NAME = ?", album.Key);
                if (result.Count > 0)
                {
                    var p = result[0];

                    // the properties' converting rules is described *below*

                    p.Songs = p.Songs + '|' + string.Join('|', album.Select(x => x.ID).Distinct());
                    if (p.AlbumArtists.IsNullorEmpty())
                    {
                        p.AlbumArtists = (from aa in album where !aa.AlbumArtists.IsNullorEmpty() select aa.AlbumArtists).FirstOrDefault();
                    }
                    if (p.AlbumArtistsSort.IsNullorEmpty())
                    {
                        p.AlbumArtistsSort = (from aa in album where !aa.AlbumArtistsSort.IsNullorEmpty() select aa.AlbumArtistsSort).FirstOrDefault();
                    }
                    if (p.Genres.IsNullorEmpty())
                    {
                        p.Genres = (from aa in album where !aa.Genres.IsNullorEmpty() select aa.Genres).FirstOrDefault();
                    }
                    if (p.PicturePath.IsNullorEmpty())
                    {
                        p.PicturePath = (from aa in album where !aa.PicturePath.IsNullorEmpty() select aa.PicturePath).FirstOrDefault();
                    }
                    if (p.Year == default(uint))
                    {
                        p.Year = album.Max(x => x.Year);
                    }
                    if (p.DiscCount == default(uint))
                    {
                        p.DiscCount = album.Max(x => x.DiscCount);
                    }
                    if (p.TrackCount == default(uint))
                    {
                        p.TrackCount = album.Max(x => x.TrackCount);
                    }
                    await conn.UpdateAsync(p);
                    newlist.Add(p);
                }
                else
                {
                    var a = new ALBUM
                    {
                        Name = album.Key,

                        // uint value, use their max value
                        DiscCount = album.Max(x => x.DiscCount),
                        TrackCount = album.Max(x => x.TrackCount),
                        Year = album.Max(x => x.Year),

                        // TODO: not combine all, just use not-null value
                        // string[] value, use their all value (remove duplicated values) combine
                        AlbumArtists = (from aa in album where !aa.AlbumArtists.IsNullorEmpty() select aa.AlbumArtists).FirstOrDefault(),//album.Where(x => !x.AlbumArtists.IsNullorEmpty()).FirstOrDefault().AlbumArtists;
                        Genres = (from aa in album where !aa.Genres.IsNullorEmpty() select aa.Genres).FirstOrDefault(),
                        AlbumArtistsSort = (from aa in album where !aa.AlbumArtistsSort.IsNullorEmpty() select aa.AlbumArtistsSort).FirstOrDefault(),

                        // normal value, use their not-null value
                        AlbumSort = (from aa in album where !aa.AlbumSort.IsNullorEmpty() select aa.AlbumSort).FirstOrDefault(),
                        ReplayGainAlbumGain = (from aa in album where aa.ReplayGainAlbumGain != double.NaN select aa.ReplayGainAlbumGain).FirstOrDefault(),
                        ReplayGainAlbumPeak = (from aa in album where aa.ReplayGainAlbumPeak != double.NaN select aa.ReplayGainAlbumPeak).FirstOrDefault(),
                        PicturePath = (from aa in album where !aa.PicturePath.IsNullorEmpty() select aa.PicturePath).FirstOrDefault(),

                        // songs, serialized as "ID0|ID1|ID2...|IDn"
                        Songs = string.Join('|', album.Select(x => x.ID).Distinct())
                    };
                    await conn.InsertAsync(a);
                    newlist.Add(a);
                }
            }
            if (newlist.Count > 0)
            {
                AlbumModified?.Invoke(this, new AlbumModifiedEventArgs(newlist.ToArray().Select(x => new Album(x)).ToArray()));
            }
        }



        public async Task<IList<Song>> GetSongsAsync(int[] ids)
        {
            var list = new List<SONG>();
            foreach (var id in ids)
            {
                var s = await conn.FindAsync<SONG>(id);
                if (s == null)
                {
                    continue;
                }
                list.Add(s);
            }
            var k = list.ConvertAll(x => new Song(x));
            var k1 = k.OrderBy(m => m.Disc);
            return k1.OrderBy(m => m.Track).ToList();
        }

        public async Task<List<FOLDER>> GetFolderAsync(string path)
        {
            return await conn.QueryAsync<FOLDER>("SELECT * FROM FOLDER WHERE PATH=?", path);
        }

        internal async Task<List<T>> GetWithQueryAsync<T>(string character, object value) where T : new()
        {
            var type = typeof(T);
            if (value == null)
            {
                return await conn.QueryAsync<T>($"SELECT * FROM {type.Name} WHERE {character} IS NULL");
            }
            return await conn.QueryAsync<T>($"SELECT * FROM {type.Name} WHERE {character} = '{value}'");
        }

        public async Task<List<Artist>> GetArtistsAsync()
        {
            return await conn.QueryAsync<Artist>("SELECT COUNT(*) AS COUNT, ALBUMARTISTS FROM SONG GROUP BY ALBUMARTISTS");
        }

        public async Task RemoveFolderAsync(int ID)
        {
            await conn.QueryAsync<object>("DELETE FROM FOLDER WHERE ID=?", ID);
        }

        internal async Task SongCountAddAsync(int id, int countToAdd)
        {
            var res = await conn.QueryAsync<STATISTICS>("SELECT * FROM STATISTICS WHERE TARGETID=? AND TARGETTYPE=0", id);
            var time = DateTime.Now;
            if (res.Count > 0)
            {
                res[0].PlayedCount += countToAdd;
                res[0].LastPlay = time;
                await conn.UpdateAsync(res[0]);
            }
            else
            {
                await conn.InsertAsync(new STATISTICS
                {
                    TargetID = id,
                    TargetType = 0,
                    PlayedCount = countToAdd,
                    LastPlay = time
                });
            }
            var resPlay = await conn.QueryAsync<PLAYSTATISTIC>("SELECT * FROM PLAYSTATISTIC WHERE TARGETID=? AND TARGETTYPE=0", id);
            if (resPlay.Count > 0)
            {
                var dateval = (int)resPlay[0].GetType().GetProperty(time.DayOfWeek.ToString()).GetValue(resPlay[0]);
                resPlay[0].GetType().GetProperty(time.DayOfWeek.ToString()).SetValue(resPlay[0], dateval + 1);
                if (time.Hour < 5)
                {
                    resPlay[0].Evening++;
                }
                else if (time.Hour < 10)
                {
                    resPlay[0].Morning++;
                }
                else if (time.Hour < 14)
                {
                    resPlay[0].Noon++;
                }
                else if (time.Hour < 19)
                {
                    resPlay[0].Afternoon++;
                }
                else if (time.Hour < 23)
                {
                    resPlay[0].Dusk++;
                }
                else
                {
                    resPlay[0].Evening++;
                }
                await conn.UpdateAsync(resPlay[0]);
            }
            else
            {
                var pS = new PLAYSTATISTIC
                {
                    TargetID = id,
                    TargetType = 0
                };
                pS.GetType().GetProperty(time.DayOfWeek.ToString()).SetValue(pS, 1);
                if (time.Hour < 5)
                {
                    pS.Evening++;
                }
                else if (time.Hour < 10)
                {
                    pS.Morning++;
                }
                else if (time.Hour < 14)
                {
                    pS.Noon++;
                }
                else if (time.Hour < 19)
                {
                    pS.Afternoon++;
                }
                else if (time.Hour < 23)
                {
                    pS.Dusk++;
                }
                await conn.InsertAsync(pS);
            }
        }

        public async Task<List<GenericMusicItem>> GetTodayListAsync()
        {
            var t = DateTime.Now;
            var day = t.DayOfWeek.ToString();
            var a = await conn.QueryAsync<PLAYSTATISTIC>($"SELECT * FROM PLAYSTATISTIC WHERE {day}>0 AND TARGETTYPE=0 ORDER BY {day} DESC LIMIT 50");
            var aRes = await conn.QueryAsync<SONG>($"SELECT * FROM SONG WHERE ID IN ({string.Join(',', a.Select(x => x.TargetID))})");


            var b = await conn.QueryAsync<PLAYSTATISTIC>($"SELECT * FROM PLAYSTATISTIC WHERE {day}>0 AND TARGETTYPE=1 ORDER BY {day} DESC LIMIT 5");
            var bRes = await conn.QueryAsync<ALBUM>($"SELECT * FROM ALBUM WHERE ID IN ({string.Join(',', b.Select(x => x.TargetID))})");


            var list = aRes.ConvertAll(x => new GenericMusicItem(x));
            list.AddRange(bRes.ConvertAll(x => new GenericMusicItem(x)));
            list.Shuffle();
            return list;
        }

        public async Task<List<GenericMusicItem>> GetFavListAsync()
        {
            var res = await conn.QueryAsync<STATISTICS>($"SELECT * FROM STATISTICS WHERE TARGETTYPE=0 AND PlayedCount>=(SELECT AVG(PlayedCount) FROM STATISTICS) ORDER BY PlayedCount DESC LIMIT 25");
            res.AddRange(await conn.QueryAsync<STATISTICS>($"SELECT * FROM STATISTICS WHERE TARGETTYPE=0 AND Favorite=1 ORDER BY RANDOM() LIMIT 25"));
            var distintedres = res.Distinct();
            var alist = await conn.QueryAsync<SONG>($"SELECT * FROM SONG WHERE ID IN ({string.Join(',', distintedres.Select(x => x.TargetID))})");

            var final = new List<GenericMusicItem>();

            var albumFromaList = from s in alist group s by s.Album;
            foreach (var item in albumFromaList)
            {
                if (item.Count() == 1)
                {
                    final.Add(new GenericMusicItem(item.First()));
                    continue;
                }
                final.Add(new GenericMusicItem(new Album(item)));
            }

            var bres = await conn.QueryAsync<STATISTICS>($"SELECT * FROM STATISTICS WHERE TARGETTYPE=0 AND PlayedCount>=(SELECT AVG(PlayedCount) FROM STATISTICS) ORDER BY PlayedCount DESC LIMIT 5");
            bres.AddRange(await conn.QueryAsync<STATISTICS>($"SELECT * FROM STATISTICS WHERE TARGETTYPE=0 AND Favorite=1 ORDER BY RANDOM() LIMIT 5"));
            var bdistintedres = bres.Distinct();
            var blist = await conn.QueryAsync<ALBUM>($"SELECT * FROM ALBUM WHERE ID IN ({string.Join(',', bdistintedres.Select(x => x.TargetID))})");

            final.AddRange(blist.ConvertAll(x =>
            {
                var songs = Array.ConvertAll(x.Songs.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries), (a) =>
                {
                    return int.Parse(a);
                });

                return new GenericMusicItem(x);
            }));
            final.Shuffle();
            return final;
        }

        public async Task<List<T>> GetRandomListAsync<T>(int count = 50) where T : new()
        {
            return await conn.QueryAsync<T>($"SELECT * FROM {typeof(T).Name} ORDER BY RANDOM() LIMIT ?", count);
        }

        internal async Task<List<T>> GetWithQueryAsync<T>(string v) where T : new()
        {
            return await conn.QueryAsync<T>(v);
        }

        internal async Task UpdateFolderAsync(FOLDER f)
        {
            await conn.InsertOrReplaceAsync(f);
        }

        internal async Task<IList<string>> GetFilePathsAsync()
        {
            return (await conn.QueryAsync<Path>("SELECT FILEPATH FROM SONG")).ConvertAll(x => x.FilePath);
        }

        internal async Task RemoveSongAsync(string path)
        {
            await conn.QueryAsync<int>("DELETE FROM SONG WHERE FILEPATH=?", path);
        }

        internal async Task<List<T>> SearchAsync<T>(string text, params string[] parameter) where T : new()
        {
            text = $"%{text.Replace(' ', '%')}%";
            var query = $"SELECT * FROM {typeof(T).Name} WHERE ";
            foreach (var item in parameter)
            {
                query += $"{item} LIKE '{text}' OR ";
            }
            query = query.Substring(0, query.Length - 4);
            query += " COLLATE NOCASE";
            return await conn.QueryAsync<T>(query);
        }

        internal async Task<List<GenericMusicItem>> GetNowListAsync()
        {
            var time = DateTime.Now;
            string day;
            if (time.Hour < 5)
            {
                day = "EVENING";
            }
            else if (time.Hour < 10)
            {
                day = "Morning";
            }
            else if (time.Hour < 14)
            {
                day = "Noon";
            }
            else if (time.Hour < 19)
            {
                day = "Afternoon";
            }
            else if (time.Hour < 23)
            {
                day = "Dusk";
            }
            else
            {
                day = "Evening";
            }


            var a = await conn.QueryAsync<PLAYSTATISTIC>($"SELECT * FROM PLAYSTATISTIC WHERE {day}>0 AND TARGETTYPE=0 ORDER BY {day} DESC LIMIT 50");
            var aRes = await conn.QueryAsync<SONG>($"SELECT * FROM SONG WHERE ID IN ({string.Join(',', a.Select(x => x.TargetID))})");

            var b = await conn.QueryAsync<PLAYSTATISTIC>($"SELECT * FROM PLAYSTATISTIC WHERE {day}>0 AND TARGETTYPE=1 ORDER BY {day} DESC LIMIT 5");
            var bRes = await conn.QueryAsync<ALBUM>($"SELECT * FROM ALBUM WHERE ID IN ({string.Join(',', b.Select(x => x.TargetID))})");

            var list = new List<GenericMusicItem>(aRes.ConvertAll(x => new GenericMusicItem(x)));
            list.AddRange(bRes.ConvertAll(x => new GenericMusicItem(x)));
            list.Shuffle();
            return list;
        }
    }

    public class Path
    {
        public string FilePath { get; set; }
    }

    public class Artist : IKey
    {
        public int Count { get; set; }
        public string AlbumArtists { get; set; }

        public string Key => AlbumArtists;
    }

}