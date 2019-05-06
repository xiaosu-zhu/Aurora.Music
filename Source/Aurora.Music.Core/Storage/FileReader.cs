// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core.Models;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TagLib;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace Aurora.Music.Core.Storage
{
    public static class FileReader
    {
        public static async Task<IReadOnlyList<StorageFile>> ReadFilesAsync(IReadOnlyList<IStorageItem> p)
        {
            var list = new List<StorageFile>();
            foreach (var item in p)
            {
                if (item is IStorageFile file)
                {
                    foreach (var types in Consts.FileTypes)
                    {
                        if (types == file.FileType)
                        {
                            list.Add(file as StorageFile);
                            break;
                        }
                    }
                }
                else if (item is StorageFolder folder)
                {
                    var options = new Windows.Storage.Search.QueryOptions
                    {
                        FolderDepth = Windows.Storage.Search.FolderDepth.Deep,
                        IndexerOption = Windows.Storage.Search.IndexerOption.DoNotUseIndexer,
                    };
                    foreach (var ext in Consts.FileTypes)
                    {
                        options.FileTypeFilter.Add(ext);
                    }
                    var query = folder.CreateFileQueryWithOptions(options);
                    list.AddRange(await query.GetFilesAsync());
                }
            }
            return list;
        }

        public static event EventHandler<ProgressReport> ProgressUpdated;
        public static event EventHandler Completed;

        public static async Task<List<Song>> GetAllSongAsync()
        {
            var opr = SQLOperator.Current();
            var songs = await opr.GetAllAsync<SONG>();
            return songs.ConvertAll(x => new Song(x));
        }

        public static async Task PlayStaticAddAsync(int id, int targetType, int addAmount)
        {
            var opr = SQLOperator.Current();
            if (targetType == 0)
            {
                await opr.SongCountAddAsync(id, addAmount);
            }
        }

        /// <summary>
        /// TODO: Only pick files which not in the database, and find deleted files to delete
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        private static async Task<IList<StorageFile>> GetFilesAsync(StorageFolder folder, IList<string> filterdFolderNames)
        {
            // TODO: determine is ondrive on demand
            var files = new List<StorageFile>();
            files.AddRange(await new FileTracker(folder, filterdFolderNames).SearchFolder());
            return files;
        }

        public static async Task<int> GetArtistsCountAsync()
        {
            var opr = SQLOperator.Current();
            var artists = await opr.GetArtistsAsync();
            return artists.Count;
        }

        public static async Task<List<Album>> GetAllAlbumsAsync()
        {
            var opr = SQLOperator.Current();

            // get aritst-associated albums
            var albums = await opr.GetAllAsync<ALBUM>();
            var res = albums.ConvertAll(a => new Album(a));

            var otherSongs = await opr.GetWithQueryAsync<SONG>($"SELECT * FROM SONG WHERE ALBUM IS NULL");

            // remove duplicated (we suppose that artist's all song is just 1000+, this way can find all song and don't take long time)
            var otherGrouping = from song in otherSongs group song by song.Album;
            // otherSongs has item
            if (!otherGrouping.IsNullorEmpty())
            {
                res.AddRange(otherGrouping.ToList().ConvertAll(a => new Album(a)));
            }
            return res;
        }

        public static async Task<int> CountAsync<T>() where T : new()
        {
            if (typeof(T) == typeof(Song))
            {
                return await SQLOperator.Current().CountAsync<SONG>();
            }
            if (typeof(T) == typeof(Album))
            {
                return await SQLOperator.Current().CountAsync<ALBUM>();
            }
            if (typeof(T) == typeof(Artist))
            {
                return await SQLOperator.Current().CountAsync<Artist>();
            }
            if (typeof(T) == typeof(PlayList))
            {
                return await SQLOperator.Current().CountAsync<PLAYLIST>();
            }
            if (typeof(T) == typeof(Podcast))
            {
                return await SQLOperator.Current().CountAsync<PODCAST>();
            }
            if (typeof(T) == typeof(StorageFolder))
            {
                return await SQLOperator.Current().CountAsync<FOLDER>();
            }
            return await SQLOperator.Current().CountAsync<T>();
        }

        public static async Task<List<GenericMusicItem>> GetFavListAsync()
        {
            return await SQLOperator.Current().GetFavListAsync();
        }

        public static async Task<List<GenericMusicItem>> GetRandomListAsync()
        {
            var opr = SQLOperator.Current();
            var p = Shared.Helpers.Tools.Random.Next(15);
            var songs = await opr.GetRandomListAsync<SONG>(25 - p);
            var albums = await opr.GetRandomListAsync<ALBUM>(p);
            var list = songs.ConvertAll(x => new GenericMusicItem(x));

            foreach (var album in albums)
            {
                var albumSongs = Array.ConvertAll(album.Songs.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries), (a) =>
                {
                    return int.Parse(a);
                });

                list.Add(new GenericMusicItem(album));
            }

            list.Shuffle();
            return list;
        }

        public static async Task<IEnumerable<ListWithKey<GenericMusicItem>>> GetHeroListAsync()
        {
            var opr = SQLOperator.Current();
            var todaySuggestion = await opr.GetTodayListAsync();
            var nowSuggestion = await opr.GetNowListAsync();
            var recent = await opr.GetRecentListAsync();
            var random = await GetRandomListAsync();

            var res = new List<ListWithKey<GenericMusicItem>>
            {
                new ListWithKey<GenericMusicItem>("Random", random),
                new ListWithKey<GenericMusicItem>(string.Format(Consts.Localizer.GetString("TodaySuggestionText"), DateTime.Today.DayOfWeek.GetDisplayName()), todaySuggestion),
                new ListWithKey<GenericMusicItem>(string.Format(Consts.Localizer.GetString("TodayFavText"), DateTime.Now.GetHourString()), nowSuggestion),
                new ListWithKey<GenericMusicItem>(Consts.Localizer.GetString("RencentlyPlayedText"), recent)
            };
            return res;
        }

        public static async Task<List<Album>> GetAlbumsAsync()
        {
            var opr = SQLOperator.Current();
            var albums = await opr.GetAllAsync<ALBUM>();
            return albums.ConvertAll(a => new Album(a));
        }


        public static List<StorageFolder> InitFolderList()
        {
            var list = new List<StorageFolder>();
            if (Settings.Current.IncludeMusicLibrary)
            {
                // TODO: music library don't have path
                list.Add(KnownFolders.MusicLibrary);
            }
            list.Add(AsyncHelper.RunSync(async () => await ApplicationData.Current.LocalFolder.CreateFolderAsync("Music", CreationCollisionOption.OpenIfExists)));
            return list;
        }

        public static async Task ReadAsync(IList<StorageFolder> folder, IList<string> filterdFolderNames)
        {
            var list = new List<StorageFile>();
            int i = 1;

            var scan = Consts.Localizer.GetString("FolderScanText");

            foreach (var item in folder)
            {
                var files = await GetFilesAsync(item, filterdFolderNames);

                var opr = SQLOperator.Current();
                if (KnownFolders.MusicLibrary.Path == item.Path || item.Path.Contains(ApplicationData.Current.LocalFolder.Path))
                {

                }
                else
                {
                    await opr.UpdateFolderAsync(item, files.Count);
                }

                list.AddRange(files);
                ProgressUpdated?.Invoke(null, new ProgressReport() { Description = SmartFormat.Smart.Format(scan, i, folder.Count), Current = i, Total = folder.Count });
                i++;
            }
            await Task.Delay(200);
            ProgressUpdated?.Invoke(null, new ProgressReport() { Description = Consts.Localizer.GetString("FolderScanFinishText"), Current = i, Total = folder.Count });
            await Task.Delay(200);
            await ReadFileandSaveAsync(from a in list group a by a.Path into b select b.First());
        }

        public static async Task ReadFileandSaveAsync(IEnumerable<StorageFile> files)
        {
            var opr = SQLOperator.Current();
            var total = files.Count();
            int i = 1;

            var newlist = new List<SONG>();

            var durationFilter = Settings.Current.FileDurationFilterEnabled;
            var duration = Convert.ToInt32(Settings.Current.FileDurationFilter);

            var scan = Consts.Localizer.GetString("FileReadText");
            var oneDriveFailed = false;
            foreach (var file in files.OrderBy(f => f.Path))
            {
                try
                {
                    if (!file.IsAvailable || file.Attributes.HasFlag(FileAttributes.LocallyIncomplete))
                    {
                        if (file.Provider.Id != "OneDrive" || oneDriveFailed || !Settings.Current.OnedriveRoaming)
                            continue;
                        try
                        {
                            var oneDriveFile = await OneDrivePropertyProvider.GetOneDriveFilesAsync(file.Path);
                            var properties = oneDriveFile.OneDriveItem;
                            var audioProp = properties.Audio;
                            var basic = properties.LastModifiedDateTime ?? DateTimeOffset.MinValue;
                            if (durationFilter && audioProp.Duration < duration)
                                continue;
                            var artist = audioProp?.Artist is null ? null : new[] { audioProp.Artist };
                            var composers = audioProp?.Composers is null ? null : new[] { audioProp.Composers };
                            var song = await Song.CreateAsync(null, file.Path, (audioProp?.Title, audioProp?.Album, artist, artist, composers, null, TimeSpan.FromMilliseconds(audioProp?.Duration ?? 0), (uint)(audioProp?.Bitrate * 1000 ?? 0), 0, basic.DateTime), null, oneDriveFile);
                            var t = await opr.InsertSongAsync(song);
                            if (t != null)
                            {
                                newlist.Add(t);
                            }
                        }
                        catch
                        {
                            // Prevent another try by next file.
                            oneDriveFailed = true;
                            // Will be handled by outer catch block.
                            throw;
                        }
                    }
                    else
                    {
                        using (var tagTemp = File.Create(file.AsAbstraction()))
                        {
                            var prop = await file.Properties.GetMusicPropertiesAsync();

                            var d = prop.Duration.Milliseconds < 1 ? tagTemp.Properties.Duration : prop.Duration;

                            if (durationFilter && d.Milliseconds < duration)
                                continue;

                            var song = await Song.CreateAsync(tagTemp.Tag, file.Path, await file.GetViolatePropertiesAsync(), tagTemp.Properties, null);
                            var t = await opr.InsertSongAsync(song);
                            if (t != null)
                            {
                                newlist.Add(t);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    continue;
                }
                finally
                {
                    ProgressUpdated?.Invoke(null, new ProgressReport() { Description = SmartFormat.Smart.Format(scan, i, total), Current = i, Total = total, CanHide = true });
                    i++;
                }
            }

            await RemoveDuplicateAsync();

            await SortAlbumsAsync();
            Completed?.Invoke(null, EventArgs.Empty);
        }

        private static async Task RemoveDuplicateAsync()
        {
            var opr = SQLOperator.Current();
            var songs = await opr.GetAllAsync<SONG>();
            var duplicates = new List<SONG>();
            for (int i = 0; i < songs.Count; i++)
            {
                for (int j = i + 1; j < songs.Count; j++)
                {
                    if (songs[i].FilePath == songs[j].FilePath)
                    {
                        duplicates.Add(songs[j]);
                        songs.Remove(songs[j]);
                    }
                }
            }
            await opr.RemoveSongsAsync(duplicates);
        }

        public static async Task UpdateSongAsync(Song model)
        {
            var opr = SQLOperator.Current();
            await opr.UpdateSongAsync(model);
        }

        public static async Task<List<Song>> GetSongsAsync()
        {
            var opr = SQLOperator.Current();
            var songs = await opr.GetAllAsync<SONG>();
            return songs.ConvertAll(a => new Song(a));
        }

        public static async Task SortAlbumsAsync()
        {
            await Task.Run(async () =>
            {
                var opr = SQLOperator.Current();
                var songs = await opr.GetAllAsync<SONG>();
                var albums = from song in songs group song by song.Album into album select album;
                var count = albums.Count();

                int i = 1;
                var scan = Consts.Localizer.GetString("AlbumSortText");

                ProgressUpdated?.Invoke(null, new ProgressReport() { Description = SmartFormat.Smart.Format(scan, 0, count), Current = 0, Total = count, CanHide = true });
                foreach (var item in albums)
                {
                    await opr.AddAlbumAsync(item);
                    ProgressUpdated?.Invoke(null, new ProgressReport() { Description = SmartFormat.Smart.Format(scan, i, count), Current = i, Total = count, CanHide = true });

                    i++;
                }
                Completed?.Invoke(null, EventArgs.Empty);
            });
        }

        public static async Task<List<GenericMusicItem>> SearchAsync(string text)
        {
            var opr = SQLOperator.Current();
            text = SQLOperator.SQLEscaping_LIKE(text);

            var songs = await opr.SearchAsync<SONG>(text, "TITLE", "PERFORMERS");

            var album = await opr.SearchAsync<ALBUM>(text, "NAME", "AlbumArtists");

            var l = new List<GenericMusicItem>(album.ConvertAll(x => new GenericMusicItem(x)));
            l.AddRange(songs.ConvertAll(x => new GenericMusicItem(x)));
            return l;
        }

        public static async Task<IList<Song>> ReadFileandSendBackAsync(List<StorageFile> files)
        {
            var tempList = new List<Song>();
            var total = files.Count;
            foreach (var file in files)
            {
                using (var tagTemp = File.Create(file.AsAbstraction()))
                {
                    tempList.Add(await Song.CreateAsync(tagTemp.Tag, file.Path, await file.GetViolatePropertiesAsync(), tagTemp.Properties, null));
                }
            }
            var result = from song in tempList orderby song.Track orderby song.Disc group song by song.Album;
            var list = new List<Song>();
            foreach (var item in result)
            {
                list.AddRange(item);
            }
            return list;
        }

        public static async Task<Song> ReadFileAsync(StorageFile file)
        {
            using (var tagTemp = File.Create(file.AsAbstraction()))
            {
                return await CreateAsync(tagTemp.Tag, file.Path, await file.GetViolatePropertiesAsync(), tagTemp.Properties);
            }
        }

        private static readonly string[] violateProperties = new string[] { "System.Music.AlbumArtist", "System.Music.Artist" };

        public static async Task<(string title, string album, string[] performer, string[] artist, string[] composer, string conductor, TimeSpan duration, uint bitrate, uint rating, DateTime lastModify)> GetViolatePropertiesAsync(this StorageFile file)
        {
            var basic = await file.GetBasicPropertiesAsync();
            var music = await file.Properties.GetMusicPropertiesAsync();
            var properties = await file.Properties.RetrievePropertiesAsync(violateProperties);
            string[] performer, artist;
            var performerProperty = properties[violateProperties[1]];
            if (performerProperty is string[] pArr)
            {
                performer = pArr;
            }
            else if (performerProperty is string p)
            {
                performer = new string[] { p };
            }
            else
            {
                performer = null;
            }
            var artistProperty = properties[violateProperties[0]];
            if (artistProperty is string a)
            {
                artist = a.Split(';', StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                artist = null;
            }
            return (music.Title, music.Album, performer, artist, music.Composers.ToArray(), music.Conductors.FirstOrDefault(), music.Duration, music.Bitrate, music.Rating, basic.DateModified.DateTime);
        }

        private static async Task<Song> CreateAsync(Tag tag, string path, (string title, string album, string[] performer, string[] artist, string[] composer, string conductor, TimeSpan duration, uint bitrate, uint rating, DateTime lastModify) music, Properties p)
        {
            var song = new Song
            {
                LastModified = music.lastModify,
                Duration = music.duration.TotalMilliseconds < 1 ? p.Duration : music.duration,
                BitRate = music.bitrate,
                FilePath = path,
                Rating = (uint)Math.Round(music.rating / 20.0),
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
                Album = music.album,
                AlbumArtists = music.artist,
                AlbumArtistsSort = tag.AlbumArtistsSort,
                AlbumSort = tag.AlbumSort,
                AmazonId = tag.AmazonId,
                Title = music.title,
                TitleSort = tag.TitleSort,
                Track = tag.Track,
                TrackCount = tag.TrackCount,
                ReplayGainTrackGain = tag.ReplayGainTrackGain,
                ReplayGainTrackPeak = tag.ReplayGainTrackPeak,
                ReplayGainAlbumGain = tag.ReplayGainAlbumGain,
                ReplayGainAlbumPeak = tag.ReplayGainAlbumPeak,
                Comment = tag.Comment,
                Disc = tag.Disc,
                Composers = music.composer,
                ComposersSort = tag.ComposersSort,
                Conductor = music.conductor,
                DiscCount = tag.DiscCount,
                Copyright = tag.Copyright,
                Genres = tag.Genres,
                Grouping = tag.Grouping,
                Lyrics = tag.Lyrics,
                Performers = music.performer,
                PerformersSort = tag.PerformersSort,
                Year = tag.Year
            };

            var pictures = tag.Pictures;
            if (!pictures.IsNullorEmpty())
            {
                var fileName = $"{CreateHash64(song.Title).ToString()}.{pictures[0].MimeType.Split('/').LastOrDefault().Replace("jpeg", "jpg")}";
                var s = await ApplicationData.Current.TemporaryFolder.TryGetItemAsync(fileName);
                if (s == null)
                {
                    var cacheImg = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteBytesAsync(cacheImg, pictures[0].Data.Data);
                    song.PicturePath = cacheImg.Path;
                }
                else
                {
                    song.PicturePath = s.Path;
                }
            }
            else
            {
                try
                {
                    var folder = await StorageFolder.GetFolderFromPathAsync(System.IO.Path.GetDirectoryName(path));
                    var result = folder.CreateFileQueryWithOptions(new Windows.Storage.Search.QueryOptions()
                    {
                        FolderDepth = Windows.Storage.Search.FolderDepth.Shallow,
                        ApplicationSearchFilter = "System.FileName:\"cover\" System.FileExtension:=(\".jpg\" OR \".png\" OR \".jpeg\" OR \".gif\" OR \".tiff\" OR \".bmp\")"
                    });
                    var files = await result.GetFilesAsync();
                    if (files.Count > 0)
                    {
                        var album = music.album;
                        if (album.IsNullorEmpty())
                        {
                            album = Consts.UnknownAlbum;
                        }
                        album = Shared.Utils.InvalidFileNameChars.Aggregate(album, (current, c) => current.Replace(c + "", "_"));
                        album = $"{album}.{pictures[0].MimeType.Split('/').LastOrDefault().Replace("jpeg", "jpg")}";
                        try
                        {
                            var s = await Consts.ArtworkFolder.TryGetItemAsync(album);
                            if (s == null)
                            {
                                var cacheImg = await files[0].CopyAsync(Consts.ArtworkFolder, album, NameCollisionOption.ReplaceExisting);
                                song.PicturePath = cacheImg.Path;
                            }
                            else
                            {
                                song.PicturePath = s.Path;
                            }
                        }
                        catch (ArgumentException)
                        {
                            song.PicturePath = string.Empty;
                        }
                    }
                    else
                    {
                        song.PicturePath = string.Empty;
                    }
                }
                catch (Exception)
                {
                    song.PicturePath = string.Empty;
                }
            }
            return song;
        }
        private static ulong CreateHash64(string str)
        {
            byte[] utf8 = System.Text.Encoding.UTF8.GetBytes(str);

            ulong value = (ulong)utf8.Length;
            for (int n = 0; n < utf8.Length; n++)
            {
                value += (ulong)utf8[n] << ((n * 5) % 56);
            }
            return value;
        }
    }

    public class ProgressReport
    {
        public string Description { get; set; }



        public int Current { get; set; }

        public int Total { get; set; }
        public bool CanHide { get; internal set; }
    }
}
