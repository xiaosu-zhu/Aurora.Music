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
using Windows.Storage;
using Windows.Storage.Search;
using Windows.Storage.Streams;

namespace Aurora.Music.Core.Storage
{
    public class FileTracker
    {
        public static event TypedEventHandler<IStorageQueryResultBase, object> FilesChanged;

        public FileTracker(StorageFolder f, IList<string> filteredFolderNames)
        {
            Folder = f;
            var options = new QueryOptions
            {
                FolderDepth = FolderDepth.Deep,
                IndexerOption = IndexerOption.DoNotUseIndexer,
                ApplicationSearchFilter = ComposeFilters(filteredFolderNames),
            };
            foreach (var ext in Consts.FileTypes)
            {
                options.FileTypeFilter.Add(ext);
            }
            Query = Folder.CreateFileQueryWithOptions(options);
            Query.ContentsChanged += Query_ContentsChanged;
        }

        private void Query_ContentsChanged(IStorageQueryResultBase sender, object args)
        {
            FilesChanged?.Invoke(sender, EventArgs.Empty);
        }

        public StorageFolder Folder { get; }
        public StorageFileQueryResult Query { get; }

        private string ComposeFilters(IList<string> filteredFolderNames)
        {
            string q = string.Empty;
            if (filteredFolderNames != null && filteredFolderNames.Count > 0)
            {
                q = "System.FolderNameDisplay:NOT";
                q += $"({string.Join(" OR ", filteredFolderNames)}) ";
            }
            if (Settings.Current.FileSizeFilterEnabled)
            {
                q += $" System.Size:>{Settings.Current.GetSystemSize()} ";
            }
            return q;
        }

        public async Task<IReadOnlyList<StorageFile>> SearchFolder()
        {
            Query.ContentsChanged -= Query_ContentsChanged;
            var files = await Query.GetFilesAsync();
            Query.ContentsChanged += Query_ContentsChanged;
            return files;
        }

        public static async Task<List<StorageFile>> FindChanges(List<StorageFile> list)
        {
            var opr = SQLOperator.Current();
            var filePaths = await opr.GetFilePathsAsync();
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

        public static async Task AddTags(IStorageFile resultFile, Song downloadSong)
        {
            using (var tagTemp = TagLib.File.Create(resultFile.AsAbstraction()))
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
    }
}
