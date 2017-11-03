using Aurora.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
                FileTypeFilter = { ".flac", ".wav", ".m4a", ".aac", ".mp3" },
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
            var folders = await Task.WhenAll(foldersDB.Select(async x => await x.GetFolderAsync()));
            var list = new List<StorageFile>();
            foreach (var item in folders)
            {
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

        void QueryContentsChanged(Windows.Storage.Search.IStorageQueryResultBase sender, object args)
        {
            FilesChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
