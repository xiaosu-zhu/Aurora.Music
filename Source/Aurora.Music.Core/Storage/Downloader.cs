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
using Windows.ApplicationModel.Background;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Web;

namespace Aurora.Music.Core.Storage
{
    public sealed class Downloader : IDisposable
    {
        private List<Tuple<DownloadOperation, CancellationTokenSource>> activeDownloads;
        private BackgroundTransferGroup notificationsGroup;
        private BackgroundDownloader downloader;

        private static Downloader current;
        public static Downloader Current
        {
            get
            {
                if (current == null)
                {
                    current = new Downloader();
                }
                return current;
            }
        }

        public event EventHandler<DownloadOperation> ProgressChanged;
        public event EventHandler<DownloadOperation> ItemCompleted;
        public event EventHandler<DownloadOperation> ProgressCancelled;

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

            BackgroundTransferCompletionGroup completionGroup = new BackgroundTransferCompletionGroup();

            BackgroundTaskBuilder builder = new BackgroundTaskBuilder
            {
                TaskEntryPoint = "Aurora.Music.Services.DownloadCompletor"
            };
            builder.SetTrigger(completionGroup.Trigger);

            BackgroundTaskRegistration taskRegistration = builder.Register();

            downloader = new BackgroundDownloader(completionGroup)
            {
                TransferGroup = notificationsGroup
            };
            activeDownloads = new List<Tuple<DownloadOperation, CancellationTokenSource>>();
            DiscoverActiveDownloadsAsync();
        }

        // Enumerate the downloads that were going on in the background while the app was closed.
        private async void DiscoverActiveDownloadsAsync()
        {
            IReadOnlyList<DownloadOperation> downloads = null;

            downloads = await BackgroundDownloader.GetCurrentDownloadsForTransferGroupAsync(notificationsGroup);


            if (downloads.Count > 0)
            {
                List<Task> tasks = new List<Task>();
                foreach (DownloadOperation download in downloads)
                {
                    // Attach progress and completion handlers.
                    tasks.Add(HandleDownloadAsync(download, false, DownloadProgress));
                }

                // Don't await HandleDownloadAsync() in the foreach loop since we would attach to the second
                // download only when the first one completed; attach to the third download when the second one
                // completes etc. We want to attach to all downloads immediately.
                // If there are actions that need to be taken once downloads complete, await tasks here, outside
                // the loop.
                // await Task.WhenAll(tasks);
            }
        }

        public List<DownloadOperation> GetAll()
        {
            return activeDownloads.Select(a => a.Item1).ToList();
        }

        public void Cancel(IEnumerable<DownloadOperation> list)
        {
            foreach (var item in list)
            {
                var p = activeDownloads.First(a => a.Item1 == item);
                p?.Item2.Cancel();
                ProgressCancelled?.Invoke(this, p.Item1);
            }
        }

        public void Pause(IEnumerable<DownloadOperation> list)
        {
            foreach (var item in list)
            {
                item.Pause();
            }
        }

        public void Resume(IEnumerable<DownloadOperation> list)
        {
            foreach (var item in list)
            {
                item.Resume();
            }
        }

        public void PauseAll()
        {
            Pause(activeDownloads.Select(a => a.Item1));
        }

        public void ResumeAll()
        {
            Resume(activeDownloads.Select(a => a.Item1));
        }

        public void CancelAll()
        {
            foreach (var item in activeDownloads)
            {
                item.Item2.Cancel();
                ProgressCancelled?.Invoke(this, item.Item1);
            }
        }

        private void DownloadProgress(DownloadOperation down)
        {
            ProgressChanged?.Invoke(this, down);
        }

        private async Task HandleDownloadAsync(DownloadOperation download, bool start, Action<DownloadOperation> progressHandler)
        {
            var g = new Tuple<DownloadOperation, CancellationTokenSource>(download, new CancellationTokenSource());
            try
            {
                // Store the download so we can pause/resume.
                activeDownloads.Add(g);

                g.Item1.CostPolicy = BackgroundTransferCostPolicy.UnrestrictedOnly;

                Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>(progressHandler);
                if (start)
                {
                    // Start the download and attach a progress handler.
                    await download.StartAsync().AsTask(g.Item2.Token, progressCallback);
                }
                else
                {
                    // The download was already running when the application started, re-attach the progress handler.
                    await download.AttachAsync().AsTask(g.Item2.Token, progressCallback);
                }

                ResponseInformation response = download.GetResponseInformation();

                // GetResponseInformation() returns null for non-HTTP transfers (e.g., FTP).
                string statusCode = response != null ? response.StatusCode.ToString() : String.Empty;
            }
            catch (TaskCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                CheckStatus(e, download);
                throw;
            }
            finally
            {
                g.Item2.Dispose();
                activeDownloads.Remove(g);
                ItemCompleted?.Invoke(this, download);
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

        public async Task<StorageFile> StartDownload(Uri downloadUri, string fileName, StorageFolder savingFolder, BackgroundTransferPriority priority = BackgroundTransferPriority.Default)
        {

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("file name is empty");
            }

            StorageFile destinationFile;

            destinationFile = await savingFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);

            BackgroundDownloader downloader = new BackgroundDownloader
            {
                TransferGroup = notificationsGroup
            };
            DownloadOperation download = downloader.CreateDownload(downloadUri, destinationFile);

            download.Priority = priority;

            // Attach progress and completion handlers.
            await HandleDownloadAsync(download, true, DownloadProgress);

            return destinationFile;
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                foreach (var item in activeDownloads)
                {
                    item.Item2.Dispose();
                }
                activeDownloads.Clear();
                activeDownloads = null;

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        ~Downloader()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(false);
        }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
