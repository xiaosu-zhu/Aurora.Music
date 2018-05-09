using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurora.Music.Core.Models;
using Aurora.Music.Core.Storage;
using Windows.ApplicationModel.Background;
using Windows.Storage;

namespace Aurora.Music.Services
{
    public sealed class FileRetriever : IBackgroundTask
    {
        private BackgroundTaskDeferral deferral;

        private IList<FileTracker> trackers = new List<FileTracker>();

        private async Task FindFileChangesAsync()
        {
            var foldersDB = await SQLOperator.Current().GetAllAsync<FOLDER>();
            var filtered = new List<string>();
            var folders = FileReader.InitFolderList();
            foreach (var fo in foldersDB)
            {
                var folder = await fo.GetFolderAsync();
                if (folders.Exists(a => a.Path == folder.Path))
                {
                    continue;
                }
                if (fo.IsFiltered)
                {
                    filtered.Add(folder.DisplayName);
                }
                else
                {
                    folders.Add(folder);
                }
            }
            try
            {
                folders.Remove(folders.Find(a => a.Path == ApplicationData.Current.LocalFolder.Path));
            }
            catch (Exception)
            {
            }

            foreach (var item in folders)
            {
                trackers.Add(new FileTracker(item, filtered));
            }

            var files = new List<StorageFile>();

            foreach (var item in trackers)
            {
                files.AddRange(await item.SearchFolder());
            }

            var addedFiles = await FileTracker.FindChanges(files);

            if (!(addedFiles.Count == 0))
            {
                await FileReader.ReadFileandSaveAsync(addedFiles);
            }
        }

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            deferral = taskInstance.GetDeferral();


            deferral.Complete();
        }
    }
}
