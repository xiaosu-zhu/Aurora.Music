// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core.Models;
using Aurora.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TagLib;
using Windows.Foundation;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Aurora.Music.Core.Storage
{
    public class FileTracker
    {
        public event EventHandler FilesChanged;

        public FileTracker(StorageFolder f)
        {
            Folder = f;
        }

        public static async Task AddTags(IStorageFile resultFile, Song downloadSong)
        {
            using (var tagTemp = TagLib.File.Create(resultFile.Path))
            {
                tagTemp.Tag.Title = downloadSong.Title;
                tagTemp.Tag.Album = downloadSong.Album;
                tagTemp.Tag.AlbumArtists = downloadSong.AlbumArtists;
                tagTemp.Tag.AlbumArtistsSort = downloadSong.AlbumArtistsSort;
                tagTemp.Tag.AlbumSort = downloadSong.AlbumSort;
                tagTemp.Tag.TitleSort = downloadSong.TitleSort;
                tagTemp.Tag.Track = downloadSong.Track;
                tagTemp.Tag.TrackCount = downloadSong.TrackCount;
                tagTemp.Tag.Disc = downloadSong.Disc;
                tagTemp.Tag.Composers = downloadSong.Composers;
                tagTemp.Tag.ComposersSort = downloadSong.ComposersSort;
                tagTemp.Tag.Conductor = downloadSong.Conductor;
                tagTemp.Tag.DiscCount = downloadSong.DiscCount;
                tagTemp.Tag.Copyright = downloadSong.Copyright;
                tagTemp.Tag.PerformersSort = downloadSong.Genres;
                tagTemp.Tag.Lyrics = downloadSong.Lyrics;
                tagTemp.Tag.Performers = downloadSong.Performers;
                tagTemp.Tag.PerformersSort = downloadSong.PerformersSort;
                tagTemp.Tag.Year = downloadSong.Year;
                if (downloadSong.PicturePath != null)
                    if (tagTemp.Tag.Pictures != null && tagTemp.Tag.Pictures.Length > 0)
                    {
                    }
                    else
                    {
                        using (var referen = await (RandomAccessStreamReference.CreateFromUri(new Uri(downloadSong.PicturePath))).OpenReadAsync())
                        {
                            var p = new List<Picture>
                            {
                                new Picture(ByteVector.FromStream(referen.AsStream()))
                            };
                            tagTemp.Tag.Pictures = p.ToArray();
                        }
                    }
                tagTemp.Save();
            }
        }

        public StorageFolder Folder { get; }

        public async Task<IReadOnlyList<StorageFile>> SearchFolder()
        {
            var options = new Windows.Storage.Search.QueryOptions
            {
                FileTypeFilter = { ".flac", ".wav", ".m4a", ".aac", ".mp3", ".wma" },
                FolderDepth = Windows.Storage.Search.FolderDepth.Deep,
                IndexerOption = Windows.Storage.Search.IndexerOption.DoNotUseIndexer,
            };
            var query = Folder.CreateFileQueryWithOptions(options);

            query.ContentsChanged += QueryContentsChanged;

            return await query.GetFilesAsync();
        }

        public static async Task<List<StorageFile>> FindChanges()
        {
            var opr = SQLOperator.Current();
            var filePaths = await opr.GetFilePathsAsync();
            var foldersDB = await opr.GetAllAsync<FOLDER>();
            var folders = FileReader.InitFolderList();
            foreach (var f in foldersDB)
            {
                StorageFolder folder = await f.GetFolderAsync();
                if (folders.Exists(a => a.Path == folder.Path))
                {
                    continue;
                }
                folders.Add(folder);
            }
            try
            {
                folders.Remove(folders.Find(a => a.Path == ApplicationData.Current.LocalFolder.Path));
            }
            catch (Exception)
            {
            }
            var list = new List<StorageFile>();
            foreach (var item in folders)
            {
                if (item == null)
                    continue;
                var options = new Windows.Storage.Search.QueryOptions
                {
                    FileTypeFilter = { ".flac", ".wav", ".m4a", ".aac", ".mp3" },
                    FolderDepth = Windows.Storage.Search.FolderDepth.Deep,
                    IndexerOption = Windows.Storage.Search.IndexerOption.DoNotUseIndexer,
                };
                var query = item.CreateFileQueryWithOptions(options);
                var files = await query.GetFilesAsync();
                list.AddRange(files);
                var t = Task.Run(async () => { await opr.UpdateFolderAsync(item, files.Count); });
            }
            list.Distinct(new StorageFileComparer());

            foreach (var path in filePaths)
            {
                try
                {
                    var file = await StorageFile.GetFileFromPathAsync(path);
                    if (list.Find(x => x.Path == file.Path) is StorageFile f)
                    {
                        list.Remove(f);
                    }
                    else
                    {
                        await opr.RemoveSongAsync(path);
                    }
                }
                catch (FileNotFoundException)
                {
                    await opr.RemoveSongAsync(path);
                }
            }
            return list;
        }

        public static async Task<StorageFile> DownloadMusic(Song song, StorageFolder folder = null)
        {
            if (song.IsOnline && song.OnlineUri?.AbsolutePath != null)
            {
                var fileName = Shared.Utils.InvalidFileNameChars.Aggregate($"{string.Join('/', (song.Performers ?? new string[] { song.Album ?? "" }))} - {song.Title}", (current, c) => current.Replace(c + "", "_"));
                fileName += song.FileType;
                if (folder == null)
                {
                    folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Download", CreationCollisionOption.OpenIfExists);
                }
                return await Downloader.Current.StartDownload(song.OnlineUri, fileName, folder, new DownloadDesc()
                {
                    Title = song.Title,
                    Description = "Song"
                });
            }
            throw new InvalidOperationException("Can't download a local file");
        }

        void QueryContentsChanged(Windows.Storage.Search.IStorageQueryResultBase sender, object args)
        {
            FilesChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
