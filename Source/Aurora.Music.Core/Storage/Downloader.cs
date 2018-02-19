// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Web;

namespace Aurora.Music.Core.Storage
{
    public sealed class Downloader
    {
        private List<DownloadOperation> activeDownloads;
        private CancellationTokenSource cts;
        private BackgroundTransferGroup notificationsGroup;

        public event EventHandler<DownloadOperation> DownloadUpdated;

        public Downloader()
        {
            // Use a unique group name so that no other component in the app uses the same group. The recommended way
            // is to generate a GUID and use it as group name as shown below.
            notificationsGroup = BackgroundTransferGroup.CreateGroup("{E37CDDEC-CFB4-4748-8314-576F1D328A60}");

            // When creating a group, we can optionally define the transfer behavior of transfers associated with the
            // group. A "parallel" transfer behavior allows multiple transfers in the same group to run concurrently
            // (default). A "serialized" transfer behavior allows at most one default priority transfer at a time for
            // the group.
            notificationsGroup.TransferBehavior = BackgroundTransferBehavior.Parallel;
        }

        // Enumerate the downloads that were going on in the background while the app was closed.
        private async Task DiscoverActiveDownloadsAsync(Action<DownloadOperation> progressHandler)
        {
            activeDownloads = new List<DownloadOperation>();

            IReadOnlyList<DownloadOperation> downloads = null;

            downloads = await BackgroundDownloader.GetCurrentDownloadsForTransferGroupAsync(notificationsGroup);


            if (downloads.Count > 0)
            {
                List<Task> tasks = new List<Task>();
                foreach (DownloadOperation download in downloads)
                {
                    // Attach progress and completion handlers.
                    tasks.Add(HandleDownloadAsync(download, false, progressHandler));
                }

                // Don't await HandleDownloadAsync() in the foreach loop since we would attach to the second
                // download only when the first one completed; attach to the third download when the second one
                // completes etc. We want to attach to all downloads immediately.
                // If there are actions that need to be taken once downloads complete, await tasks here, outside
                // the loop.
                await Task.WhenAll(tasks);
            }
        }

        private void DownloadProgress(DownloadOperation down)
        {

        }

        private async Task HandleDownloadAsync(DownloadOperation download, bool start, Action<DownloadOperation> progressHandler)
        {
            try
            {
                // Store the download so we can pause/resume.
                activeDownloads.Add(download);

                Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>(progressHandler);
                if (start)
                {
                    // Start the download and attach a progress handler.
                    await download.StartAsync().AsTask(cts.Token, progressCallback);
                }
                else
                {
                    // The download was already running when the application started, re-attach the progress handler.
                    await download.AttachAsync().AsTask(cts.Token, progressCallback);
                }

                ResponseInformation response = download.GetResponseInformation();

                // GetResponseInformation() returns null for non-HTTP transfers (e.g., FTP).
                string statusCode = response != null ? response.StatusCode.ToString() : String.Empty;
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception e)
            {
                CheckStatus(e, download);
            }
            finally
            {
                activeDownloads.Remove(download);
            }
        }

        private bool CheckStatus(Exception e, DownloadOperation download = null)
        {
            WebErrorStatus error = BackgroundTransferError.GetStatus(e.HResult);
            if (error == WebErrorStatus.Unknown)
            {
                return false;
            }

            if (download == null)
            {

            }
            else
            {

            }

            return true;
        }

        public async Task StartDownload(Uri downloadUri, string fileName, StorageFolder savingFolder, Action<DownloadOperation> progressHandler, BackgroundTransferPriority priority = BackgroundTransferPriority.Default)
        {

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("file name is empty");
            }

            StorageFile destinationFile;

            destinationFile = await savingFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);

            BackgroundDownloader downloader = new BackgroundDownloader();
            DownloadOperation download = downloader.CreateDownload(downloadUri, destinationFile);

            download.Priority = priority;

            // Attach progress and completion handlers.
            await HandleDownloadAsync(download, true, progressHandler);
        }
    }
}
