// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core.Models;
using Aurora.Shared.Extensions;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TagLib;
using Windows.Storage;

namespace Aurora.Music.Core.Storage
{
    class ALBUMComparer : IEqualityComparer<ALBUM>
    {
        public bool Equals(ALBUM x, ALBUM y)
        {
            return x.ID == y.ID;
        }

        public int GetHashCode(ALBUM obj)
        {
            return obj.GetHashCode();
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

    public class DownloadDesc
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public Guid Guid { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }

    public class AVATAR
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        [Unique]
        public string Artist { get; set; }

        public string Uri { get; set; }
    }

    public class SEARCHHISTORY
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        [Unique]
        public string Query { get; set; }
    }

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
    }

    class SONG
    {
        private string albumArtists;

        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        public SONG() { }


        public SONG(Song song)
        {
            LastModified = song.LastModified;
            IsOneDrive = song.IsOnedrive;
            ID = song.ID;
            FilePath = song.FilePath;
            Duration = song.Duration;
            BitRate = song.BitRate;
            Rating = song.Rating;
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
            Performers = string.Join(Consts.ArraySeparator, song.Performers ?? new string[] { });
            PerformersSort = string.Join(Consts.ArraySeparator, song.PerformersSort ?? new string[] { });
            AlbumArtists = string.Join(Consts.ArraySeparator, song.AlbumArtists ?? new string[] { });
            AlbumArtistsSort = string.Join(Consts.ArraySeparator, song.AlbumArtistsSort ?? new string[] { });
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
            Composers = string.Join(Consts.ArraySeparator, song.Composers ?? new string[] { });
            ComposersSort = string.Join(Consts.ArraySeparator, song.ComposersSort ?? new string[] { });
            Conductor = song.Conductor;
            DiscCount = song.DiscCount;
            Copyright = song.Copyright;
            Genres = string.Join(Consts.ArraySeparator, song.Genres ?? new string[] { });
            Grouping = song.Grouping;
            Lyrics = song.Lyrics;
            Year = song.Year;
            PicturePath = song.PicturePath;
        }

        public DateTime LastModified { get; set; }

        public bool IsOneDrive { get; set; }

        public double Rating { get; set; }

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
            Genres = string.Join(Consts.ArraySeparator, album.Genres ?? new string[] { });
            Year = album.Year;
            AlbumSort = album.AlbumSort;
            TrackCount = album.TrackCount;
            DiscCount = album.DiscCount;
            AlbumArtists = string.Join(Consts.ArraySeparator, album.AlbumArtists);
            AlbumArtistsSort = string.Join(Consts.ArraySeparator, album.AlbumArtistsSort ?? new string[] { });
            ReplayGainAlbumGain = album.ReplayGainAlbumGain;
            ReplayGainAlbumPeak = album.ReplayGainAlbumPeak;
            PicturePath = album.PicturePath;
            IsOnedrive = album.IsOnedrive;
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

        public bool IsOnedrive { get; set; }
    }

    class PLAYLIST
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        [Unique]
        public string Title { get; set; }
        public string HeroArtworks { get; set; }
        public string Description { get; set; }
        public string Tags { get; set; }
        public string IDs { get; set; }



        public PLAYLIST() { }

        public PLAYLIST(PlayList p)
        {
            ID = p.ID;
            Title = p.Title;
            Description = p.Description;
            Tags = string.Join(Consts.ArraySeparator, p.Tags ?? new string[] { });
            HeroArtworks = string.Join(Consts.ArraySeparator, p.HeroArtworks ?? new string[] { });
            IDs = string.Join('|', p.SongsID ?? new int[] { });
        }
    }

    public class PODCAST
    {
        public PODCAST() { }
        public PODCAST(Podcast podcast)
        {
            ID = podcast.ID;
            Title = podcast.Title;
            Author = podcast.Author;
            Description = podcast.Description;
            XMLPath = podcast.XMLPath;
            LastUpdate = podcast.LastUpdate;
            Subscribed = podcast.Subscribed;
            SortRevert = podcast.SortRevert;
            HeroArtworks = string.Join(Consts.ArraySeparator, podcast.HeroArtworks ?? new string[] { });
            XMLUrl = podcast.XMLUrl;
        }

        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        [Unique]
        public string Title { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string XMLPath { get; set; }
        public string XMLUrl { get; set; }
        public DateTime LastUpdate { get; set; }
        public DateTime ReadBefore { get; set; }
        public bool Subscribed { get; set; }
        public bool SortRevert { get; set; }
        public string HeroArtworks { get; set; }
    }

    public class ONLINESONG
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        public string OnlineID { get; set; }
        public string OnlineUrl { get; set; }
        public string OnlineAlbumID { get; set; }

        public string Title { get; set; }
        public string Album { get; set; }
        public string Performers { get; set; }
        public string AlbumArtists { get; set; }
        public string PicturePath { get; set; }
        public uint BitRate { get; set; }
        public uint Year { get; set; }
        public uint Track { get; set; }
        public uint TrackCount { get; set; }
        public TimeSpan Duration { get; set; }
        public string FileType { get; set; }
    }

    public class FOLDER
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        public int SongsCount { get; set; }
        public string Token { get; set; }

        public bool IsFiltered { get; set; }

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

        protected virtual async void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                if (conn != null)
                {
                    await conn.CloseAsync();
                    conn = null;
                }
                GC.Collect();

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
            //GC.SuppressFinalize(this);
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
        public static string SQLEscaping_LIKE(string value)
        {
            return value.Replace("'", "''").Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");
        }

        private SQLOperator()
        {
            conn = new SQLiteAsyncConnection(DB_PATH);
            CreateTable();
        }

        private void CreateTable()
        {
            conn.GetConnection().CreateTables(CreateFlags.None, new Type[] { typeof(SONG), typeof(ALBUM), typeof(FOLDER), typeof(STATISTICS), typeof(PLAYSTATISTIC), typeof(AVATAR), typeof(PLAYLIST), typeof(SEARCHHISTORY), typeof(PODCAST), typeof(DownloadDesc) });
        }

        public async Task WriteFavoriteAsync(int id, bool isCurrentFavorite)
        {
            var res = await conn.QueryAsync<STATISTICS>("SELECT * FROM STATISTICS WHERE TARGETID=? AND TARGETTYPE=0", id);
            if (res.Count > 0)
            {
                res[0].Favorite = isCurrentFavorite;
                await conn.UpdateAsync(res[0]);
            }
            else
            {
                await conn.InsertAsync(new STATISTICS
                {
                    TargetID = id,
                    TargetType = 0,
                    Favorite = isCurrentFavorite,
                });
            }
        }

        internal async Task UpdateSongRatingAsync(int id, double rat)
        {
            var result = await conn.QueryAsync<SONG>("SELECT * FROM SONG WHERE ID = ?", id);
            if (result.Count > 0)
            {
                result[0].Rating = rat;
                await conn.UpdateAsync(result[0]);
            }
            else
            {
                return;
            }
        }

        public async Task<bool> AddFolderAsync(StorageFolder folder, bool filtered)
        {
            var token = await conn.QueryAsync<FOLDER>("SELECT * FROM FOLDER WHERE PATH=?", folder.Path);
            if (token.Count > 0)
            {
                token[0].IsFiltered = filtered;
                await conn.UpdateAsync(token[0]);
                return false;
            }
            else
            {
                var f = new FOLDER(folder)
                {
                    IsFiltered = filtered
                };
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

        internal async Task<bool> GetFavoriteAsync(int id)
        {
            var res = await conn.QueryAsync<STATISTICS>("SELECT * FROM STATISTICS WHERE TARGETID=? AND TARGETTYPE=0", id);
            if (res.Count > 0)
            {
                return res[0].Favorite;
            }
            else
            {
                return false;
            }
        }

        public async Task<List<int>> GetFavoriteAsync()
        {
            var list = await conn.QueryAsync<STATISTICS>("SELECT * FROM STATISTICS WHERE Favorite=1 AND TARGETTYPE=0");
            return list.ConvertAll(a => a.ID);
        }

        internal async Task<SONG> InsertSongAsync(Song song)
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
        /*
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
                    p.AlbumArtists = string.Join(Consts.ArraySeparator, (from aa in album where !aa.AlbumArtists.IsNullorEmpty() group aa by aa.AlbumArtists into g select g.Key).SelectMany(a => a).Distinct(StringComparer.InvariantCultureIgnoreCase));
                }
                if (p.AlbumArtistsSort.IsNullorEmpty())
                {
                    p.AlbumArtistsSort = string.Join(Consts.ArraySeparator, (from aa in album where !aa.AlbumArtistsSort.IsNullorEmpty() group aa by aa.AlbumArtistsSort into g select g.Key).SelectMany(a => a).Distinct(StringComparer.InvariantCultureIgnoreCase));
                }
                if (p.Genres.IsNullorEmpty())
                {
                    p.Genres = string.Join(Consts.ArraySeparator, (from aa in album where !aa.Genres.IsNullorEmpty() group aa by aa.Genres into g select g.Key).SelectMany(a => a).Distinct(StringComparer.InvariantCultureIgnoreCase));
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

                    AlbumArtists = string.Join(Consts.ArraySeparator, (from aa in album where !aa.AlbumArtists.IsNullorEmpty() group aa by aa.AlbumArtists into g select g.Key).SelectMany(f => f).Distinct(StringComparer.InvariantCultureIgnoreCase)),
                    Genres = string.Join(Consts.ArraySeparator, (from aa in album where !aa.Genres.IsNullorEmpty() group aa by aa.Genres into g select g.Key).SelectMany(f => f).Distinct(StringComparer.InvariantCultureIgnoreCase)),
                    AlbumArtistsSort = string.Join(Consts.ArraySeparator, (from aa in album where !aa.AlbumArtistsSort.IsNullorEmpty() group aa by aa.AlbumArtistsSort into g select g.Key).SelectMany(f => f).Distinct(StringComparer.InvariantCultureIgnoreCase)),

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

                        AlbumArtists = (from aa in album where !aa.AlbumArtists.IsNullorEmpty() select aa.AlbumArtists).FirstOrDefault(),
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
        */

        internal async Task AddAlbumAsync(IGrouping<string, SONG> album)
        {
            var result = await conn.QueryAsync<ALBUM>("SELECT * FROM ALBUM WHERE NAME = ?", album.Key);
            if (result.Count > 0)
            {
                var p = result[0];

                // the properties' converting rules is described *below*
                /*p.Songs = string.Join('|', album.Select(x => x.ID).Distinct());
                if (p.AlbumArtists.IsNullorEmpty())
                {
                    p.AlbumArtists = string.Join(Consts.ArraySeparator, (from aa in album where !aa.AlbumArtists.IsNullorEmpty() group aa by aa.AlbumArtists into g select g.Key).Distinct(StringComparer.InvariantCultureIgnoreCase));
                }
                if (p.AlbumArtistsSort.IsNullorEmpty())
                {
                    p.AlbumArtistsSort = string.Join(Consts.ArraySeparator, (from aa in album where !aa.AlbumArtistsSort.IsNullorEmpty() group aa by aa.AlbumArtistsSort into g select g.Key).Distinct(StringComparer.InvariantCultureIgnoreCase));
                }
                if (p.Genres.IsNullorEmpty())
                {
                    p.Genres = string.Join(Consts.ArraySeparator, (from aa in album where !aa.Genres.IsNullorEmpty() group aa by aa.Genres into g select g.Key).Distinct(StringComparer.InvariantCultureIgnoreCase));
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
                */

                var a = new ALBUM
                {
                    Name = album.Key,

                    // uint value, use their max value
                    DiscCount = album.Max(x => x.DiscCount),
                    TrackCount = album.Max(x => x.TrackCount),
                    Year = album.Max(x => x.Year),

                    // uint value, merge their items
                    AlbumArtists = string.Join(Consts.ArraySeparator, album.SelectMany(aa => aa.AlbumArtists?.Split(Consts.ArraySeparator, StringSplitOptions.RemoveEmptyEntries) ?? Enumerable.Empty<string>()).Distinct(StringComparer.InvariantCultureIgnoreCase)),
                    Genres = string.Join(Consts.ArraySeparator, album.SelectMany(aa => aa.Genres?.Split(Consts.ArraySeparator, StringSplitOptions.RemoveEmptyEntries) ?? Enumerable.Empty<string>()).Distinct(StringComparer.InvariantCultureIgnoreCase)),
                    AlbumArtistsSort = string.Join(Consts.ArraySeparator, album.SelectMany(aa => aa.AlbumArtistsSort?.Split(Consts.ArraySeparator, StringSplitOptions.RemoveEmptyEntries) ?? Enumerable.Empty<string>()).Distinct(StringComparer.InvariantCultureIgnoreCase)),

                    // normal value, use their not-null value
                    AlbumSort = (from aa in album where !aa.AlbumSort.IsNullorEmpty() select aa.AlbumSort).FirstOrDefault(),
                    ReplayGainAlbumGain = (from aa in album where aa.ReplayGainAlbumGain != double.NaN select aa.ReplayGainAlbumGain).FirstOrDefault(),
                    ReplayGainAlbumPeak = (from aa in album where aa.ReplayGainAlbumPeak != double.NaN select aa.ReplayGainAlbumPeak).FirstOrDefault(),
                    PicturePath = (from aa in album where !aa.PicturePath.IsNullorEmpty() select aa.PicturePath).FirstOrDefault(),

                    // songs, serialized as "ID0|ID1|ID2...|IDn"
                    Songs = string.Join('|', album.Select(x => x.ID).Distinct()),
                    IsOnedrive = album.Any(s => s.IsOneDrive)
                };
                a.ID = p.ID;
                await conn.UpdateAsync(a);
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

                    // uint value, merge their items
                    AlbumArtists = string.Join(Consts.ArraySeparator, album.SelectMany(aa => aa.AlbumArtists?.Split(Consts.ArraySeparator, StringSplitOptions.RemoveEmptyEntries) ?? Enumerable.Empty<string>()).Distinct(StringComparer.InvariantCultureIgnoreCase)),
                    Genres = string.Join(Consts.ArraySeparator, album.SelectMany(aa => aa.Genres?.Split(Consts.ArraySeparator, StringSplitOptions.RemoveEmptyEntries) ?? Enumerable.Empty<string>()).Distinct(StringComparer.InvariantCultureIgnoreCase)),
                    AlbumArtistsSort = string.Join(Consts.ArraySeparator, album.SelectMany(aa => aa.AlbumArtistsSort?.Split(Consts.ArraySeparator, StringSplitOptions.RemoveEmptyEntries) ?? Enumerable.Empty<string>()).Distinct(StringComparer.InvariantCultureIgnoreCase)),

                    // normal value, use their not-null value
                    AlbumSort = (from aa in album where !aa.AlbumSort.IsNullorEmpty() select aa.AlbumSort).FirstOrDefault(),
                    ReplayGainAlbumGain = (from aa in album where aa.ReplayGainAlbumGain != double.NaN select aa.ReplayGainAlbumGain).FirstOrDefault(),
                    ReplayGainAlbumPeak = (from aa in album where aa.ReplayGainAlbumPeak != double.NaN select aa.ReplayGainAlbumPeak).FirstOrDefault(),
                    PicturePath = (from aa in album where !aa.PicturePath.IsNullorEmpty() select aa.PicturePath).FirstOrDefault(),

                    // songs, serialized as "ID0|ID1|ID2...|IDn"
                    Songs = string.Join('|', album.Select(x => x.ID).Distinct()),
                    IsOnedrive = album.Any(s => s.IsOneDrive)
                };
                await conn.InsertAsync(a);
            }
        }


        internal async Task<SONG> GetSongAsync(int id)
        {
            return await conn.FindAsync<SONG>(id);
        }

        public async Task<List<Song>> GetSongsAsync(IEnumerable<int> ids)
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

        public async Task<List<Album>> GetAlbumsOfArtistAsync(string value)
        {
            if (value.IsNullorEmpty())
            {
                // anonymous artists, get their songs
                var songs = await conn.QueryAsync<SONG>($"SELECT * FROM SONG WHERE ALBUMARTISTS IS NULL OR PERFORMERS IS NULL OR PERFORMERS ='{value}'");
                var albumGrouping = from song in songs group song by song.Album;
                return albumGrouping.ToList().ConvertAll(a => new Album(a));
            }

            value = SQLEscaping_LIKE(value);

            // get aritst-associated albums
            // This version of SQLite-Net can't parameterize LIKE
            var albums = (await conn.QueryAsync<ALBUM>($"SELECT * FROM ALBUM WHERE ALBUMARTISTS LIKE '{value}{Consts.ArraySeparator}%' OR ALBUMARTISTS LIKE '%{Consts.ArraySeparator}{value}{Consts.ArraySeparator}%' OR ALBUMARTISTS LIKE '%{Consts.ArraySeparator}{value}' OR ALBUMARTISTS LIKE '{value}'")).Distinct(new ALBUMComparer()).ToList();
            var res = albums.ConvertAll(a => new Album(a));

            var otherSongs = await conn.QueryAsync<SONG>($"SELECT * FROM SONG WHERE PERFORMERS LIKE '{value}{Consts.ArraySeparator}%' OR PERFORMERS LIKE '%{Consts.ArraySeparator}{value}{Consts.ArraySeparator}%' OR PERFORMERS LIKE '%{Consts.ArraySeparator}{value}' OR PERFORMERS = '{value}' OR ALBUMARTISTS LIKE '{value}{Consts.ArraySeparator}%' OR ALBUMARTISTS LIKE '%{Consts.ArraySeparator}{value}{Consts.ArraySeparator}%' OR ALBUMARTISTS LIKE '%{Consts.ArraySeparator}{value}' OR ALBUMARTISTS LIKE '{value}'");

            // remove duplicated (we suppose that artist's all song is just 1000+, this way can find all song and don't take long time)
            otherSongs.RemoveAll(x => albums.Exists(b => b.Name == x.Album));
            var otherGrouping = from song in otherSongs group song by song.Album;
            // otherSongs has item
            if (!otherGrouping.IsNullorEmpty())
            {
                res.AddRange(otherGrouping.ToList().ConvertAll(a => new Album(a)));
            }
            return res;
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
            var artists = await conn.QueryAsync<Artist>("SELECT COUNT(*) AS COUNT, ALBUMARTISTS FROM SONG GROUP BY ALBUMARTISTS");
            var res = new List<Artist>();
            for (int i = 0; i < artists.Count; i++)
            {
                var arr = artists[i].AlbumArtists.Split(new string[] { Consts.ArraySeparator }, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length <= 0)
                {
                    res.Add(artists[i]);
                }
                else
                {
                    foreach (var item in arr)
                    {
                        try
                        {
                            var ext = res.Find(a => a.AlbumArtists.Equals(item, StringComparison.InvariantCultureIgnoreCase));
                            if (ext != null)
                            {
                                ext.Count += artists[i].Count;
                            }
                            else
                            {
                                res.Add(new Artist()
                                {
                                    AlbumArtists = item,
                                    Count = artists[i].Count
                                });
                            }
                        }
                        catch (Exception)
                        {
                            res.Add(new Artist()
                            {
                                AlbumArtists = item,
                                Count = artists[i].Count
                            });
                        }
                    }
                }
            }
            return res;
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


        internal async Task<List<GenericMusicItem>> GetRecentListAsync(int count = 50)
        {
            var res = await conn.QueryAsync<STATISTICS>("SELECT * FROM STATISTICS WHERE TARGETTYPE=0 AND LASTPLAY>0 ORDER BY LASTPLAY DESC LIMIT ?", count);
            var alist = await conn.QueryAsync<SONG>($"SELECT * FROM SONG WHERE ID IN ({string.Join(',', res.Select(x => x.TargetID))})");

            var final = new List<GenericMusicItem>();

            foreach (var item in alist)
            {
                final.Add(new GenericMusicItem(item));
            }
            final.Shuffle();
            return final;
        }

        public async Task<List<GenericMusicItem>> GetFavListAsync()
        {
            var res = await conn.QueryAsync<STATISTICS>($"SELECT * FROM STATISTICS WHERE TARGETTYPE=0 AND PlayedCount>0 ORDER BY PlayedCount DESC LIMIT 25");
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

            var bres = await conn.QueryAsync<STATISTICS>($"SELECT * FROM STATISTICS WHERE TARGETTYPE=1 AND PlayedCount>0 ORDER BY PlayedCount DESC LIMIT 5");
            bres.AddRange(await conn.QueryAsync<STATISTICS>($"SELECT * FROM STATISTICS WHERE TARGETTYPE=1 AND Favorite=1 ORDER BY RANDOM() LIMIT 5"));
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
            try
            {
                var s = await conn.QueryAsync<SONG>("SELECT * FROM SONG WHERE FILEPATH=?", path);
                if (s != null && s.Count > 0)
                {
                    foreach (var song in s)
                    {
                        await conn.DeleteAsync<SONG>(song.ID);
                        await conn.QueryAsync<int>("DELETE FROM STATISTICS WHERE TargetID=?;DELETE FROM PLAYSTATISTIC WHERE TargetID=?", song.ID, song.ID);
                    }
                }
            }
            catch (Exception)
            {

            }
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

        public async Task RemoveAlbumAsync(int iD)
        {
            await conn.QueryAsync<ALBUM>("DELETE FROM ALBUM WHERE ID = ?", iD);
        }

        internal async Task UpdateAlbumAsync(ALBUM s)
        {
            await conn.UpdateAsync(s);
        }

        public async Task UpdateAlbumAsync(Album s)
        {
            await conn.UpdateAsync(new ALBUM(s));
        }

        public async Task<Album> GetAlbumByNameAsync(string album, int songID)
        {
            if (album.IsNullorEmpty())
            {
                return null;
            }
            else
            {
                var res = await conn.QueryAsync<ALBUM>("SELECT * FROM ALBUM WHERE NAME=?", album);
                if (res.Count > 0)
                {
                    foreach (var item in res)
                    {
                        if (item.Songs.Contains(songID.ToString()))
                        {
                            return new Album(item);
                        }
                    }
                }
            }
            return null;
        }

        public async Task<Album> GetAlbumByNameAsync(string album)
        {
            if (album.IsNullorEmpty())
            {
                return null;
            }
            else
            {
                var res = await conn.QueryAsync<ALBUM>("SELECT * FROM ALBUM WHERE NAME=?", album);
                if (res.Count > 0)
                {
                    return new Album(res[0]);
                }
            }
            return null;
        }

        public async Task<Album> GetAlbumByIDAsync(int contextualID)
        {
            var res = await conn.QueryAsync<ALBUM>("SELECT * FROM ALBUM WHERE ID=?", contextualID);
            if (res.Count > 0)
            {
                return new Album(res[0]);
            }
            return null;
        }

        public async Task UpdateAvatarAsync(string artist, string originalString)
        {
            var res = await conn.QueryAsync<AVATAR>("SELECT * FROM AVATAR WHERE Artist=?", artist);
            if (res.Count > 0)
            {
                if (res[0].Uri == originalString)
                {
                    return;
                }
                res[0].Uri = originalString;
                await conn.UpdateAsync(res[0]);
                return;
            }
            await conn.InsertAsync(new AVATAR()
            {
                Artist = artist,
                Uri = originalString
            });
        }

        public async Task<string> GetAvatarAsync(string rawName)
        {
            var res = await conn.QueryAsync<AVATAR>("SELECT * FROM AVATAR WHERE Artist=?", rawName);
            if (res.Count > 0)
            {
                return res[0].Uri;
            }
            return null;
        }

        public Task UpdateAlbumArtworkAsync(int id, string originalString)
        {
            // TODO: UpdateAlbumArtworkAsync
            return Task.Run(() => { });
        }

        public async Task DANGER_DROP_ALL()
        {
            await conn.DropTableAsync<SONG>();
            await conn.DropTableAsync<ALBUM>();
            await conn.DropTableAsync<FOLDER>();
            await conn.DropTableAsync<STATISTICS>();
            await conn.DropTableAsync<PLAYSTATISTIC>();
            await conn.DropTableAsync<AVATAR>();
            await conn.DropTableAsync<PLAYLIST>();
            await conn.DropTableAsync<SEARCHHISTORY>();
            await conn.DropTableAsync<PODCAST>();
            await conn.DropTableAsync<DownloadDesc>();
        }

        internal async Task UpdateSongAsync(Song model)
        {
            var t = new SONG(model);
            await conn.UpdateAsync(t);
        }

        public async Task<PlayList> GetPlayListAsync(int ID)
        {
            var res = await conn.QueryAsync<PLAYLIST>("SELECT * FROM PLAYLIST WHERE ID=?", ID);
            if (res.Count > 0)
            {
                return new PlayList(res[0]);
            }
            var fav = new PlayList()
            {
                Title = Consts.Localizer.GetString("Favorites"),
                Description = Consts.Localizer.GetString("AutoGen"),
            };

            var favSongs = await conn.QueryAsync<STATISTICS>("SELECT * FROM STATISTICS WHERE Favorite=1");
            if (favSongs.Count > 0)
            {
                var favsongID = await conn.QueryAsync<SONG>($"SELECT * FROM SONG WHERE ID IN ({string.Join(',', favSongs.Select(x => x.TargetID))})");
                fav.SongsID = favsongID.Select(x => x.ID).ToArray();
                var artworks = from g in favsongID group g by g.Album into p orderby p.Count() descending select p.First().PicturePath;
                fav.HeroArtworks = artworks.Take(3).ToArray();
            }
            else
            {
                fav.SongsID = new int[] { };
            }
            return fav;
        }

        // NOTE: treat favorite songs(in STATISTIC) as a playlist
        public async Task<List<PlayList>> GetPlayListBriefAsync()
        {
            var fav = new PlayList()
            {
                Title = Consts.Localizer.GetString("Favorites"),
                Description = Consts.Localizer.GetString("AutoGen"),
            };

            var favSongs = await conn.QueryAsync<STATISTICS>("SELECT * FROM STATISTICS WHERE Favorite=1");
            if (favSongs.Count > 0)
            {
                var favsongID = await conn.QueryAsync<SONG>($"SELECT * FROM SONG WHERE ID IN ({string.Join(',', favSongs.Select(x => x.TargetID))})");
                fav.SongsID = favsongID.Select(x => x.ID).ToArray();
                var artworks = from g in favsongID group g by g.Album into p orderby p.Count() descending select p.First().PicturePath;
                fav.HeroArtworks = artworks.Take(3).ToArray();
            }
            else
            {
                fav.SongsID = new int[] { };
            }


            var list = await conn.QueryAsync<PLAYLIST>("SELECT * FROM PLAYLIST");
            var playlists = list.ConvertAll(x =>
            {
                return new PlayList(x);
            });
            playlists.Insert(0, fav);
            return playlists;
        }

        public async Task<List<PODCAST>> GetPodcastListBriefAsync()
        {
            return await conn.QueryAsync<PODCAST>("SELECT * FROM PODCAST WHERE Subscribed=1");
        }

        internal async Task<int> UpdatePlayListAsync(PLAYLIST p)
        {
            var res = await conn.QueryAsync<PLAYLIST>("SELECT * FROM PLAYLIST WHERE TITLE=?", p.Title);
            if (res.Count > 0)
            {
                await conn.UpdateAsync(p);
                return p.ID;
            }
            else
            {
                await conn.InsertAsync(p);
                return p.ID;
            }
        }

        public async Task SaveSearchHistoryAsync(string query)
        {
            var res = await conn.QueryAsync<SEARCHHISTORY>("SELECT * FROM SEARCHHISTORY WHERE QUERY=?", query);
            if (res.Count > 0)
            {
                foreach (var item in res)
                {
                    await conn.DeleteAsync(item);
                }
            }
            await conn.InsertAsync(new SEARCHHISTORY()
            {
                Query = query
            });
        }

        public async Task DeleteSearchHistoryAsync(string query)
        {
            var res = await conn.QueryAsync<int>("DELETE FROM SEARCHHISTORY WHERE QUERY=?", query);
        }

        public async Task<List<SEARCHHISTORY>> GetSearchHistoryAsync()
        {
            return await conn.QueryAsync<SEARCHHISTORY>("SELECT * FROM SEARCHHISTORY ORDER BY ID DESC LIMIT 100");
        }

        internal async Task<int> UpdatePodcastAsync(PODCAST p)
        {
            var res = await conn.QueryAsync<PODCAST>("SELECT * FROM PODCAST WHERE TITLE=? AND AUTHOR=?", p.Title, p.Author);
            if (res.Count > 0)
            {
                p.ID = res[0].ID;
                await conn.UpdateAsync(p);
                return p.ID;
            }
            await conn.InsertAsync(p);

            return p.ID;
        }

        internal async Task<PODCAST> TryGetPODCAST(string xMLUrl)
        {
            var res = await conn.QueryAsync<PODCAST>("SELECT * FROM PODCAST WHERE XMLUrl=?", xMLUrl);
            if (res.Count > 0)
            {
                return res[0];
            }
            return null;
        }

        internal async Task<T> GetItemByIDAsync<T>(int iD) where T : new()
        {
            var res = await conn.QueryAsync<T>($"SELECT * FROM {typeof(T).Name} WHERE ID=?", iD);
            if (res.Count > 0)
            {
                return res[0];
            }
            return default(T);
        }

        internal async Task RemoveEmptyAlbumAsync()
        {
            await conn.QueryAsync<int>("DELETE FROM ALBUM WHERE SONGS IS NULL");
        }

        public async Task RemovePlayListAsync(int iD)
        {
            await conn.QueryAsync<int>("DELETE FROM PLAYLIST WHERE ID=?", iD);
        }

        public async Task RemoveDownloadDes(DownloadDesc item)
        {
            await conn.QueryAsync<int>("DELETE FROM DownloadDesc WHERE ID=?", item.ID);
        }

        public async Task AddDownloadDes(DownloadDesc des)
        {
            await conn.InsertAsync(des);
        }

        public async Task UpdateDownload(DownloadDesc p)
        {
            await conn.UpdateAsync(p);
        }

        internal async Task RemoveSongsAsync(List<SONG> duplicates)
        {
            foreach (var item in duplicates)
            {
                await conn.DeleteAsync(item);
            }
        }

        public async Task<int> CountAsync<T>() where T : new()
        {
            return await conn.Table<T>().CountAsync();
        }
    }
}
