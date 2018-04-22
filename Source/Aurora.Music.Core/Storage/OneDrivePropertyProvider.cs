using Aurora.Shared.Extensions;
using Microsoft.Toolkit.Services.OneDrive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using SysPath = System.IO.Path;

namespace Aurora.Music.Core.Storage
{
    public static class OneDrivePropertyProvider
    {
        private static OneDriveStorageFolder rootFolder;

        private static CacheData cache;
        private sealed class CacheData
        {
            public List<OneDriveStorageFile> Files;
            public OneDriveStorageFolder Folder;
            public string FolderName;

        }

        public static async Task<OneDriveStorageFile> GetOneDriveFilesAsync(string fileName)
        {
            if (rootFolder is null)
            {
                OneDriveService.ServicePlatformInitializer = new Microsoft.Toolkit.Uwp.Services.OneDrive.Platform.OneDriveServicePlatformInitializer();
                OneDriveService.Instance.Initialize("daf2a654-5b98-4954-bd03-d09a375628ed", new[] { Microsoft.Toolkit.Services.Services.MicrosoftGraph.MicrosoftGraphScope.FilesReadAll });
                if (!await OneDriveService.Instance.LoginAsync())
                    throw new InvalidOperationException("Failed to log on");
                if (rootFolder is null)
                {
                    var root = await OneDriveService.Instance.RootFolderAsync();
                    Interlocked.CompareExchange(ref rootFolder, root, null);
                }
            }
            fileName = fileName.Split(@"\OneDrive\", 2).Last();
            var folderName = SysPath.GetDirectoryName(fileName);
            var sFileName = SysPath.GetFileName(fileName);
            if (folderName.IsNullorEmpty())
                return await rootFolder.GetFileAsync(fileName);

            var file = default(OneDriveStorageFile);

            // copy to local to avoid changes by other threads
            var currentCache = cache;
            if (currentCache != null && folderName == currentCache.FolderName)
                file = currentCache.Files.Find(f => f.Name == sFileName);
            if (file != null)
                return file;

            // cache miss
            var folder = await rootFolder.GetFolderAsync(folderName);
            var files = await folder.GetFilesAsync(10000);
            file = files.Find(f => f.Name == sFileName);
            cache = new CacheData
            {
                Files = files,
                Folder = folder,
                FolderName = folderName,
            };
            if (file != null)
                return file;

            // slow route: first 10000 files missed
            return await folder.GetFileAsync(sFileName);
        }

        public static async Task<string> GetThumbnail(OneDriveStorageFile file)
        {
            var thumb = await file.GetThumbnailSetAsync();
            return thumb.Source ?? thumb.Large ?? thumb.Medium ?? thumb.Small;
        }
    }
}
