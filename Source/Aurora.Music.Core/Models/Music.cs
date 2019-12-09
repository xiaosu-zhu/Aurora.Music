// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core.Storage;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using Microsoft.Toolkit.Services.OneDrive;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TagLib;
using Windows.Storage;
using Windows.System.Threading;

namespace Aurora.Music.Core.Models
{
    public enum MediaType
    {
        Song, Album, PlayList, Artist, Podcast, Placeholder
    }

    public sealed class Artist
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Uri AvatarUri { get; set; }
    }

    public sealed class AlbumInfo
    {
        public Uri AltArtwork { get; set; }
        public string Name { get; set; }
        public string Artist { get; set; }
        public uint Year { get; set; }
        public string Description { get; set; }
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
            Addtional = s.Performers.IsNullorEmpty() ? Consts.UnknownArtists : string.Join(Consts.CommaSeparator, s.Performers.Split(new string[] { Consts.ArraySeparator }, StringSplitOptions.RemoveEmptyEntries));
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
            Description = SmartFormat.Smart.Format(Consts.Localizer.GetString("SmartSongs"), ids.Length);
            Addtional = s.AlbumArtists.IsNullorEmpty() ? Consts.UnknownArtists : string.Join(Consts.CommaSeparator, s.AlbumArtists.Split(new string[] { Consts.ArraySeparator }, StringSplitOptions.RemoveEmptyEntries));

            var songIDs = AsyncHelper.RunSync(async () => await SQLOperator.Current().GetSongsAsync(ids));
            var s1 = songIDs.OrderBy(a => a.Track);
            s1 = s1.OrderBy(a => a.Disc);
            IDs = s1.Select(b => b.ID).ToArray();

            var t = ThreadPool.RunAsync(async work =>
            {
                s.Songs = string.Join('|', songIDs.Select(x => x.ID.ToString()));
                if (s.Songs.IsNullorEmpty())
                {
                    await SQLOperator.Current().RemoveAlbumAsync(s.ID);
                }
                else
                {
                    await SQLOperator.Current().UpdateAlbumAsync(s);
                }
            });

            PicturePath = s.PicturePath;
        }

        public GenericMusicItem() { }


        internal GenericMusicItem(Album s)
        {
            InnerType = MediaType.Album;
            ContextualID = s.ID;
            Title = s.Name;
            var ids = s.Songs;
            Description = SmartFormat.Smart.Format(Consts.Localizer.GetString("SmartSongs"), ids.Length);
            Addtional = s.AlbumArtists.IsNullorEmpty() ? Consts.UnknownArtists : string.Join(Consts.CommaSeparator, s.AlbumArtists);

            var songIDs = AsyncHelper.RunSync(async () => await SQLOperator.Current().GetSongsAsync(ids));
            var s1 = songIDs.OrderBy(a => a.Track);
            s1 = s1.OrderBy(a => a.Disc);
            IDs = s1.Select(b => b.ID).ToArray();


            var t = ThreadPool.RunAsync(async work =>
            {
                s.Songs = songIDs.Select(x => x.ID).ToArray();
                if (s.Songs.IsNullorEmpty())
                {
                    await SQLOperator.Current().RemoveAlbumAsync(s.ID);
                }
                else
                {
                    await SQLOperator.Current().UpdateAlbumAsync(new ALBUM(s));
                }
            });

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

    public sealed class Song
    {
        public bool IsIDEqual(Song s)
        {
            if (s == null)
            {
                return false;
            }
            if (s.IsOnline != IsOnline)
            {
                return false;
            }
            if (IsOnline)
            {
                return s.OnlineUri.Equals(OnlineUri);
            }
            return ID == s.ID && FilePath == s.FilePath;
        }

        public Song() { }

        internal Song(SONG song)
        {
            ID = song.ID;
            LastModified = song.LastModified;
            IsOnedrive = song.IsOneDrive;
            Duration = song.Duration;
            BitRate = song.BitRate;
            Rating = song.Rating;
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
            AlbumArtists = song.AlbumArtists.Split(new string[] { Consts.ArraySeparator }, StringSplitOptions.RemoveEmptyEntries);
            AlbumArtistsSort = song.AlbumArtistsSort.Split(new string[] { Consts.ArraySeparator }, StringSplitOptions.RemoveEmptyEntries);
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
            Composers = song.Composers.Split(new string[] { Consts.ArraySeparator }, StringSplitOptions.RemoveEmptyEntries);
            ComposersSort = song.ComposersSort.Split(new string[] { Consts.ArraySeparator }, StringSplitOptions.RemoveEmptyEntries);
            Conductor = song.Conductor;
            DiscCount = song.DiscCount;
            Copyright = song.Copyright;
            Genres = song.Genres.Split(new string[] { Consts.ArraySeparator }, StringSplitOptions.RemoveEmptyEntries);
            Grouping = song.Grouping;
            Lyrics = song.Lyrics;
            Performers = song.Performers.Split(new string[] { Consts.ArraySeparator }, StringSplitOptions.RemoveEmptyEntries);
            PerformersSort = song.PerformersSort.Split(new string[] { Consts.ArraySeparator }, StringSplitOptions.RemoveEmptyEntries);
            Year = song.Year;
            PicturePath = song.PicturePath;
            SampleRate = song.SampleRate;
            AudioChannels = song.AudioChannels;
        }

        public static async Task<Song> CreateAsync(Tag tag, string path, (string title, string album, string[] performer, string[] artist, string[] composer, string conductor, TimeSpan duration, uint bitrate, uint rating, DateTime lastModify) music, Properties p, OneDriveStorageFile oneDriveFile)
        {
            var graphAudio = oneDriveFile?.OneDriveItem?.Audio;
            var s = new Song();
            await s.UpdatePropertiesAsync(tag, path, music, p, oneDriveFile);
            return s;
        }


        private static async Task<string> FindCoverPictureAsync(string album, string filePath)
        {
            try
            {
                var folder = await StorageFolder.GetFolderFromPathAsync(System.IO.Path.GetDirectoryName(filePath));
                var result = folder.CreateFileQueryWithOptions(new Windows.Storage.Search.QueryOptions()
                {
                    FolderDepth = Windows.Storage.Search.FolderDepth.Shallow,
                    ApplicationSearchFilter = "System.FileName:\"cover\" System.FileExtension:=(\".jpg\" OR \".png\" OR \".jpeg\" OR \".gif\" OR \".tiff\" OR \".bmp\")"
                });
                var files = await result.GetFilesAsync();
                if (files.Count > 0)
                {
                    if (album.IsNullorEmpty())
                    {
                        album = Consts.UnknownAlbum;
                    }
                    album = Shared.Utils.InvalidFileNameChars.Aggregate(album, (current, c) => current.Replace(c + "", "_"));
                    album = $"{album}.{files[0].FileType}";
                    try
                    {
                        var cacheImg = await files[0].CopyAsync(Consts.ArtworkFolder, album, NameCollisionOption.ReplaceExisting);
                        return cacheImg.Path;
                    }
                    catch (ArgumentException)
                    {
                        return string.Empty;
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private static async Task<string> GetPicturePathAsync(IPicture[] pictures, string album, string filePath)
        {
            if (!pictures.IsNullorEmpty())
            {
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
                        var cacheImg = await Consts.ArtworkFolder.CreateFileAsync(album, CreationCollisionOption.ReplaceExisting);
                        await FileIO.WriteBytesAsync(cacheImg, pictures[0].Data.Data);
                        return cacheImg.Path;
                    }
                    else
                    {
                        return s.Path;
                    }
                }
                catch (ArgumentException)
                {
                    return string.Empty;
                }
            }
            else
            {
                try
                {
                    var folder = await StorageFolder.GetFolderFromPathAsync(System.IO.Path.GetDirectoryName(filePath));
                    var result = folder.CreateFileQueryWithOptions(new Windows.Storage.Search.QueryOptions()
                    {
                        FolderDepth = Windows.Storage.Search.FolderDepth.Shallow,
                        ApplicationSearchFilter = "System.FileName:\"cover\" System.FileExtension:=(\".jpg\" OR \".png\" OR \".jpeg\" OR \".gif\" OR \".tiff\" OR \".bmp\")"
                    });
                    var files = await result.GetFilesAsync();
                    if (files.Count > 0)
                    {
                        if (album.IsNullorEmpty())
                        {
                            album = Consts.UnknownAlbum;
                        }
                        album = Shared.Utils.InvalidFileNameChars.Aggregate(album, (current, c) => current.Replace(c + "", "_"));
                        album = $"{album}.{files[0].FileType}";
                        try
                        {
                            var cacheImg = await files[0].CopyAsync(Consts.ArtworkFolder, album, NameCollisionOption.ReplaceExisting);
                            return cacheImg.Path;
                        }
                        catch (ArgumentException)
                        {
                            return string.Empty;
                        }
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }
        }

        private static async Task<string> GetPicturePathAsync(OneDriveStorageFile file, string album)
        {
            if (file is null)
                return string.Empty;
            if (album.IsNullorEmpty())
            {
                album = Consts.UnknownAlbum;
            }
            album = Shared.Utils.InvalidFileNameChars.Aggregate(album, (current, c) => current.Replace(c + "", "_"));
            album = $"{album}.jpg";
            try
            {
                var s = await Consts.ArtworkFolder.TryGetItemAsync(album);
                if (s == null)
                {
                    var thumb = await OneDrivePropertyProvider.GetThumbnail(file);
                    if (thumb.IsNullorEmpty())
                        return string.Empty;
                    return thumb;
                }
                else
                {
                    return s.Path;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        public async Task WriteRatingAsync(uint r)
        {
            if (IsOnline)
            {
                throw new NotImplementedException("WriteRatingAsync on online");
            }
            else
            {
                var file = await StorageFile.GetFileFromPathAsync(FilePath);
                var prop = await file.Properties.GetMusicPropertiesAsync();

                prop.Rating = r;
                await prop.SavePropertiesAsync();
                Rating = (r < 0 ? 0 : r);
                await SQLOperator.Current().UpdateSongRatingAsync(ID, r / 20.0);
            }
        }

        public async Task WriteRatingAsync(double rat)
        {
            if (IsOnline)
            {
                throw new NotImplementedException("WriteRatingAsync on online");
            }
            else
            {
                var file = await StorageFile.GetFileFromPathAsync(FilePath);
                var prop = await file.Properties.GetMusicPropertiesAsync();
                uint r;
                if (rat < 0)
                {
                    r = 0;
                }
                else
                {
                    if (rat < 1)
                    {
                        r = 0;
                    }
                    else if (rat < 1.5)
                    {
                        r = 19;
                    }
                    else if (rat < 2.5)
                    {
                        r = 39;
                    }
                    else if (rat < 3.5)
                    {
                        r = 59;
                    }
                    else if (rat < 4.5)
                    {
                        r = 79;
                    }
                    else
                    {
                        r = 99;
                    }
                }
                prop.Rating = r;
                await prop.SavePropertiesAsync();
                Rating = (rat < 0 ? 0 : rat);
                await SQLOperator.Current().UpdateSongRatingAsync(ID, Rating);
            }
        }

        public async void WriteFavAsync(bool isCurrentFavorite)
        {
            if (IsOnline)
            {
                // TODO: throw new NotImplementedException("WriteFav on online");
                return;
            }
            else
            {
                await SQLOperator.Current().WriteFavoriteAsync(ID, isCurrentFavorite);
            }
        }

        public TimeSpan Duration { get; set; }
        public uint BitRate { get; set; }

        public string FilePath { get; set; }
        public string PicturePath { get; set; }

        [JsonIgnore]
        public string MusicBrainzReleaseId { get; set; }
        [JsonIgnore]
        public string MusicBrainzDiscId { get; set; }
        [JsonIgnore]
        public string MusicIpId { get; set; }
        [JsonIgnore]
        public string AmazonId { get; set; }
        [JsonIgnore]
        public string MusicBrainzReleaseStatus { get; set; }
        [JsonIgnore]
        public string MusicBrainzReleaseType { get; set; }
        [JsonIgnore]
        public string MusicBrainzReleaseCountry { get; set; }
        [JsonIgnore]
        public double ReplayGainTrackGain { get; set; }
        [JsonIgnore]
        public double ReplayGainTrackPeak { get; set; }
        [JsonIgnore]
        public double ReplayGainAlbumGain { get; set; }
        [JsonIgnore]
        public double ReplayGainAlbumPeak { get; set; }
        //public IPicture[] Pictures { get; set; }
        [JsonIgnore]
        public string FirstAlbumArtist { get; set; }
        [JsonIgnore]
        public string FirstAlbumArtistSort { get; set; }
        [JsonIgnore]
        public string FirstPerformer { get; set; }
        [JsonIgnore]
        public string FirstPerformerSort { get; set; }
        [JsonIgnore]
        public string FirstComposerSort { get; set; }
        [JsonIgnore]
        public string FirstComposer { get; set; }
        [JsonIgnore]
        public string FirstGenre { get; set; }
        [JsonIgnore]
        public string JoinedAlbumArtists { get; set; }
        [JsonIgnore]
        public string JoinedPerformers { get; set; }
        [JsonIgnore]
        public string JoinedPerformersSort { get; set; }
        [JsonIgnore]
        public string JoinedComposers { get; set; }
        [JsonIgnore]
        public string MusicBrainzTrackId { get; set; }
        [JsonIgnore]
        public string MusicBrainzReleaseArtistId { get; set; }
        [JsonIgnore]
        public bool IsEmpty { get; set; }
        [JsonIgnore]
        public string MusicBrainzArtistId { get; set; }
        [JsonIgnore]
        public TagTypes TagTypes { get; set; }
        public string Title { get; set; }
        public string TitleSort { get; set; }
        public string[] Performers { get; set; }
        public string[] PerformersSort { get; set; }
        public string[] AlbumArtists { get; set; }
        public string[] AlbumArtistsSort { get; set; }
        public string[] Composers { get; set; }
        public string[] ComposersSort { get; set; }
        public string Album { get; set; }
        [JsonIgnore]
        public string JoinedGenres { get; set; }
        public string AlbumSort { get; set; }
        public string[] Genres { get; set; }
        public uint Year { get; set; }
        public uint Track { get; set; }
        public uint TrackCount { get; set; }
        public uint Disc { get; set; }
        public uint DiscCount { get; set; }
        [JsonIgnore]
        public string Lyrics { get; set; }
        [JsonIgnore]
        public string Grouping { get; set; }
        [JsonIgnore]
        public uint BeatsPerMinute { get; set; }
        public string Conductor { get; set; }
        public string Copyright { get; set; }
        public string Comment { get; set; }
        public int ID { get; set; }
        public int SampleRate { get; set; }
        public int AudioChannels { get; set; }


        public bool IsOnline { get; set; }
        public bool IsAvaliable { get; set; } = true;
        public Uri OnlineUri { get; set; }
        public string OnlineID { get; set; }
        public double Rating { get; set; }
        public string OnlineAlbumID { get; set; }

        public bool IsPodcast { get; set; }
        public bool IsVideo { get; set; }

        public string FileType { get; set; }
        public DateTime PubDate { get; set; }
        public bool IsOnedrive { get; set; }
        public DateTime LastModified { get; set; }
        public StorageFile File { get; set; }

        public async Task<bool> GetFavoriteAsync()
        {
            if (IsOnline)
            {
                // TODO: get fac online
                return false;
            }
            else
            {
                return await SQLOperator.Current().GetFavoriteAsync(ID);
            }
        }

        public static async Task<Song> GetAsync(int songID)
        {
            return new Song(await SQLOperator.Current().GetSongAsync(songID));
        }
        public static async Task<IList<Song>> GetAsync(IEnumerable<int> songID)
        {
            return await SQLOperator.Current().GetSongsAsync(songID);
        }

        public string GetFileName()
        {
            var f = FilePath.TakeLast(FilePath.Length - FilePath.LastIndexOf('/') - 1);
            return Shared.Utils.InvalidFileNameChars.Aggregate(string.Concat(f), (current, c) => current.Replace(c + "", "_"));
        }

        public async Task UpdatePropertiesAsync(Tag tag, string path, (string title, string album, string[] performer, string[] artist, string[] composer, string conductor, TimeSpan duration, uint bitrate, uint rating, DateTime lastModify) music, Properties p, OneDriveStorageFile oneDriveFile)
        {
            var graphAudio = oneDriveFile?.OneDriveItem?.Audio;

            LastModified = music.lastModify;
            IsOnedrive = oneDriveFile != null;
            Duration = (TimeSpan)mergeData(d => d is TimeSpan dd && dd.TotalMilliseconds > 0, new TimeSpan(),
                music.duration,
                p?.Duration,
                TimeSpan.FromMilliseconds(graphAudio?.Duration ?? 0));
            BitRate = mergeUint(music.bitrate, (uint?)(p?.AudioBitrate * 1000), (uint?)(graphAudio?.Bitrate * 1000));
            AudioChannels = p?.AudioChannels ?? 0;
            SampleRate = p?.AudioSampleRate ?? 0;
            FilePath = path;
            Rating = (uint)Math.Round(music.rating / 20.0);
            MusicBrainzArtistId = tag?.MusicBrainzArtistId;
            MusicBrainzDiscId = tag?.MusicBrainzDiscId;
            MusicBrainzReleaseArtistId = tag?.MusicBrainzReleaseArtistId;
            MusicBrainzReleaseCountry = tag?.MusicBrainzReleaseCountry;
            MusicBrainzReleaseId = tag?.MusicBrainzReleaseId;
            MusicBrainzReleaseStatus = tag?.MusicBrainzReleaseStatus;
            MusicBrainzReleaseType = tag?.MusicBrainzReleaseType;
            MusicBrainzTrackId = tag?.MusicBrainzTrackId;
            MusicIpId = tag?.MusicIpId;
            BeatsPerMinute = tag?.BeatsPerMinute ?? 0;
            Album = mergeStr(music.album, tag?.Album);
            AlbumArtists = mergeArr(music.artist, tag?.AlbumArtists);
            AlbumArtistsSort = tag?.AlbumArtistsSort;
            AlbumSort = tag?.AlbumSort;
            AmazonId = tag?.AmazonId;
            Title = mergeStr(music.title, tag.Title, System.IO.Path.GetFileNameWithoutExtension(path));
            TitleSort = tag?.TitleSort;
            Track = mergeUint(tag?.Track, ((uint?)graphAudio?.Track));
            TrackCount = mergeUint(tag?.TrackCount, ((uint?)graphAudio?.TrackCount));
            ReplayGainTrackGain = tag?.ReplayGainTrackGain ?? double.NaN;
            ReplayGainTrackPeak = tag?.ReplayGainTrackPeak ?? double.NaN;
            ReplayGainAlbumGain = tag?.ReplayGainAlbumGain ?? double.NaN;
            ReplayGainAlbumPeak = tag?.ReplayGainAlbumPeak ?? double.NaN;
            Comment = tag?.Comment;
            Disc = mergeUint(tag?.Disc, ((uint?)graphAudio?.Disc));
            Composers = mergeArr(music.composer, tag?.Composers);
            ComposersSort = tag?.ComposersSort;
            Conductor = mergeStr(music.conductor, tag?.Conductor);
            DiscCount = mergeUint(tag?.DiscCount, ((uint?)graphAudio?.DiscCount));
            Copyright = tag?.Copyright;
            Genres = mergeArr(tag?.Genres, asArray(graphAudio?.Genre));
            Grouping = tag?.Grouping;
            Lyrics = tag?.Lyrics;
            Performers = mergeArr(music.performer, tag?.Performers);
            PerformersSort = tag?.PerformersSort;
            Year = mergeUint(tag?.Year, ((uint?)graphAudio?.Year));

            if (tag != null)
                PicturePath = await GetPicturePathAsync(tag.Pictures, Album, path);
            else if (oneDriveFile != null)
                PicturePath = await GetPicturePathAsync(oneDriveFile, Album);
            else
                PicturePath = await FindCoverPictureAsync(Album, path);


            T mergeData<T>(Predicate<T> isValidValue, T def, params T[] candidates)
            {
                Debug.Assert(candidates != null && candidates.Length > 0);
                var result = Array.FindIndex(candidates, isValidValue);
                if (result < 0)
                    return def;
                return candidates[result];
            }

            string mergeStr(params string[] candidates) => mergeData<string>(st => st is string ss && !string.IsNullOrWhiteSpace(ss), "", candidates);
            uint mergeUint(params uint?[] candidates) => mergeData<uint?>(ui => ui is uint uu && uu != 0, 0, candidates).GetValueOrDefault();
            T[] mergeArr<T>(params T[][] candidates) => mergeData(arr => arr is T[] a && a.Length != 0, Array.Empty<T>(), candidates);

            T[] asArray<T>(T value) where T : class => value is null ? null : new[] { value };
        }


        public async Task UpdateAsync(Tag tag, Properties p, StorageFile file)
        {
            if (ID == 0 || IsOnline)
            {
                return;
            }
            var music = await file.GetViolatePropertiesAsync();
            await UpdatePropertiesAsync(tag, file.Path, music, p, null);
            await SQLOperator.Current().UpdateSongAsync(this);
        }
    }


    public sealed class Album
    {
        public Album()
        {
        }

        public Album(int iD)
        {
            ID = iD;
        }

        internal Album(ALBUM album)
        {
            ID = album.ID;
            var songs = album.Songs.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            Songs = Array.ConvertAll(songs, (a) =>
            {
                return int.Parse(a);
            });
            Name = album.Name;
            Genres = album.Genres.IsNullorEmpty() ? null : album.Genres.Split(new string[] { Consts.ArraySeparator }, StringSplitOptions.RemoveEmptyEntries);
            Year = album.Year;
            AlbumSort = album.AlbumSort;
            TrackCount = album.TrackCount;
            DiscCount = album.DiscCount;
            AlbumArtists = album.AlbumArtists.IsNullorEmpty() ? null : album.AlbumArtists.Split(new string[] { Consts.ArraySeparator }, StringSplitOptions.RemoveEmptyEntries);
            AlbumArtistsSort = album.AlbumArtistsSort.IsNullorEmpty() ? null : album.AlbumArtistsSort.Split(new string[] { Consts.ArraySeparator }, StringSplitOptions.RemoveEmptyEntries);
            ReplayGainAlbumGain = album.ReplayGainAlbumGain;
            ReplayGainAlbumPeak = album.ReplayGainAlbumPeak;
            PicturePath = album.PicturePath;
        }

        internal Album(IGrouping<string, SONG> album)
        {
            Name = album.Key.IsNullorEmpty() ? Consts.UnknownAlbum : album.Key;

            // uint value, use their max value
            DiscCount = album.Max(x => x.DiscCount);
            TrackCount = album.Max(x => x.TrackCount);
            Year = album.Max(x => x.Year);

            // TODO: not combine all, just use not-null value
            // string[] value, use their all value (remove duplicated values) combine
            AlbumArtists = (from aa in album where !aa.AlbumArtists.IsNullorEmpty() select aa.AlbumArtists).FirstOrDefault()?.Split(new string[] { Consts.ArraySeparator }, StringSplitOptions.RemoveEmptyEntries);//album.Where(x => !x.AlbumArtists.IsNullorEmpty()).FirstOrDefault().AlbumArtists;
            Genres = (from aa in album where !aa.Genres.IsNullorEmpty() select aa.Genres).FirstOrDefault()?.Split(new string[] { Consts.ArraySeparator }, StringSplitOptions.RemoveEmptyEntries);
            AlbumArtistsSort = (from aa in album where !aa.AlbumArtistsSort.IsNullorEmpty() select aa.AlbumArtistsSort).FirstOrDefault()?.Split(new string[] { Consts.ArraySeparator }, StringSplitOptions.RemoveEmptyEntries);

            // normal value, use their not-null value
            AlbumSort = (from aa in album where !aa.AlbumSort.IsNullorEmpty() select aa.AlbumSort).FirstOrDefault();
            ReplayGainAlbumGain = (from aa in album where aa.ReplayGainAlbumGain != double.NaN select aa.ReplayGainAlbumGain).FirstOrDefault();
            ReplayGainAlbumPeak = (from aa in album where aa.ReplayGainAlbumPeak != double.NaN select aa.ReplayGainAlbumPeak).FirstOrDefault();
            PicturePath = (from aa in album where !aa.PicturePath.IsNullorEmpty() select aa.PicturePath).FirstOrDefault();

            // songs, serialized as "ID0|ID1|ID2...|IDn"
            Songs = album.Select(x => x.ID).Distinct().ToArray();

            IsOnedrive = album.Any(a => a.IsOneDrive);
        }

        public bool IsOnedrive { get; set; }

        public int[] Songs { get; set; }

        public string PicturePath { get; set; }

        public string Name { get; set; }
        public string[] Genres { get; set; }
        public uint Year { get; set; }
        public string AlbumSort { get; set; }
        public uint TrackCount { get; set; }
        public uint DiscCount { get; set; }
        public string[] AlbumArtists { get; set; }
        public string[] AlbumArtistsSort { get; set; }
        public double ReplayGainAlbumGain { get; set; }
        public double ReplayGainAlbumPeak { get; set; }
        public int ID { get; }
        public string Decsription { get; set; }
        public bool IsOnline { get; set; }
        public string[] OnlineIDs { get; set; }
        public List<Song> SongItems { get; set; }
        public string[] OnlineArtistIDs { get; set; }
    }
}
