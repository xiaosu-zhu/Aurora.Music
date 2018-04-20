using Microsoft.Toolkit.Services.OneDrive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Aurora.Music.Core.Storage
{
    public static class OneDrivePropertyProvider
    {
        private static OneDriveStorageFolder rootFolder;

        public static async Task<OneDriveStorageFile> GetOneDriveFilesAsync(string fileName)
        {
            if (rootFolder is null)
            {
                OneDriveService.ServicePlatformInitializer = new Microsoft.Toolkit.Uwp.Services.OneDrive.Platform.OneDriveServicePlatformInitializer();
                OneDriveService.Instance.Initialize("daf2a654-5b98-4954-bd03-d09a375628ed", new[] { Microsoft.Toolkit.Services.Services.MicrosoftGraph.MicrosoftGraphScope.FilesReadAll });
                if (!await OneDriveService.Instance.LoginAsync())
                    throw new InvalidOperationException("Failed to log on");
                rootFolder = await OneDriveService.Instance.RootFolderAsync();
            }
            fileName = fileName.Split(@"\OneDrive\").Last();
            return await rootFolder.GetFileAsync(fileName);
        }

        public static async Task<string> GetThumbnail(OneDriveStorageFile file)
        {
            var thumb = await file.GetThumbnailSetAsync();
            return thumb.Source ?? thumb.Large ?? thumb.Medium ?? thumb.Small;
        }
    }
}
