using Aurora.Music.Core.Models;
using Aurora.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;

namespace Aurora.Music.Core.Storage
{
    public class FileTracker
    {
        public event EventHandler FilesChanged;

        public FileTracker(StorageFolder f)
        {
            Folder = f;
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

        public static async Task<IEnumerable<StorageFile>> FindChanges()
        {
            var opr = SQLOperator.Current();
            var filePaths = await opr.GetFilePathsAsync();
            var foldersDB = await opr.GetAllAsync<FOLDER>();
            var folders = new List<StorageFolder>();
            foreach (var f in foldersDB)
            {
                folders.Add(await f.GetFolderAsync());
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

                list.AddRange(await query.GetFilesAsync());
            }
            foreach (var path in filePaths)
            {
                try
                {
                    var file = await StorageFile.GetFileFromPathAsync(path);
                    if (list.Find(x => x.Path == file.Path) is StorageFile f)
                    {
                        list.Remove(f);
                    }
                }
                catch (FileNotFoundException)
                {
                    await opr.RemoveSongAsync(path);
                }
            }
            return list;
        }

        public static async Task<IAsyncOperationWithProgress<DownloadOperation, DownloadOperation>> DownloadMusic(Song song)
        {
            if (song.IsOnline && song.OnlineUri?.AbsolutePath != null)
            {
                var fileName = Shared.Utils.InvalidFileNameChars.Aggregate(song.Title, (current, c) => current.Replace(c + "", "_"));
                return await WebHelper.DownloadFileAsync(fileName, song.OnlineUri);
            }
            throw new InvalidOperationException("Can't download a local file");
        }

        void QueryContentsChanged(Windows.Storage.Search.IStorageQueryResultBase sender, object args)
        {
            FilesChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
