// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core.Storage;
using Aurora.Music.Core.Tools;
using Aurora.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Networking.BackgroundTransfer;

namespace Aurora.Music.Services
{
    public sealed class DownloadCompletor : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var d = taskInstance.GetDeferral();

            //await FileIOHelper.AppendLogtoCacheAsync("DownloadCompletor start");

            var sa = await SQLOperator.Current().GetAllAsync<DownloadDesc>();

            BackgroundTransferCompletionGroupTriggerDetails details = taskInstance.TriggerDetails
                as BackgroundTransferCompletionGroupTriggerDetails;

            if (details == null)
            {
                // This task was not triggered by a completion group.
                return;
            }

            List<DownloadOperation> failedDownloads = new List<DownloadOperation>();
            foreach (DownloadOperation download in details.Downloads)
            {
                if (IsFailed(download))
                {
                    failedDownloads.Add(download);
                }
                else
                {
                    try
                    {
                        //await FileIOHelper.AppendLogtoCacheAsync($"success of {download.ResultFile.Name}");
                        var p = sa.Find(a => a.Guid == download.Guid);
                        sa.Remove(p);
                        Toast.SendDownload(p);
                        await SQLOperator.Current().RemoveDownloadDes(p);
                        //await FileIOHelper.AppendLogtoCacheAsync($"send toast of {download.ResultFile.Name}");
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            if (failedDownloads.Count > 0)
            {
                //await RetryDownloads(failedDownloads, sa);
            }

            await FileIOHelper.AppendLogtoCacheAsync("Complete");

            d.Complete();
        }

        private bool IsFailed(DownloadOperation download)
        {
            BackgroundTransferStatus status = download.Progress.Status;
            if (status == BackgroundTransferStatus.Error || status == BackgroundTransferStatus.Canceled)
            {
                return true;
            }

            ResponseInformation response = download.GetResponseInformation();
            if (response.StatusCode != 200)
            {
                return true;
            }

            return false;
        }

        private async Task RetryDownloads(IEnumerable<DownloadOperation> downloads, List<DownloadDesc> list)
        {
            BackgroundDownloader downloader = CreateBackgroundDownloader();
            foreach (DownloadOperation download in downloads)
            {
                try
                {
                    DownloadOperation download1 = downloader.CreateDownload(download.RequestedUri, download.ResultFile);
                    Task<DownloadOperation> startTask = download1.StartAsync().AsTask();
                    var p = list.Find(a => a.Guid == download.Guid);
                    p.Guid = download1.Guid;
                    await SQLOperator.Current().UpdateDownload(p);
                    list.Remove(p);
                }
                catch (Exception)
                {
                    continue;
                }
            }

            downloader.CompletionGroup.Enable();
        }

        private BackgroundDownloader CreateBackgroundDownloader()
        {
            // Use a unique group name so that no other component in the app uses the same group. The recommended way
            // is to generate a GUID and use it as group name as shown below.
            var notificationsGroup = BackgroundTransferGroup.CreateGroup("{E37CDDEC-CFB4-4748-8314-576F1D328A60}");

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

            return new BackgroundDownloader(completionGroup)
            {
                TransferGroup = notificationsGroup
            };
        }
    }
}
