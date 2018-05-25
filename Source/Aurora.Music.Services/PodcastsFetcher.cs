// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Aurora.Music.Core.Models;
using Aurora.Music.Core.Storage;
using Aurora.Music.Core.Tools;

using Windows.ApplicationModel.Background;

namespace Aurora.Music.Services
{
    public sealed class PodcastsFetcher : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            var posts = await SQLOperator.Current().GetPodcastListBriefAsync();
            var tasks = new List<Task>();
            foreach (var item in posts)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var p = new Podcast(item);
                    try
                    {
                        if (await p.FindUpdated() && Settings.Current.IsPodcastToast)
                        {
                            Toast.SendPodcast(p);
                            Tile.UpdatePodcast($"podcast{p.ID}", p);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }));
            }
            await Task.WhenAll(tasks);
            deferral.Complete();
        }
    }
}
