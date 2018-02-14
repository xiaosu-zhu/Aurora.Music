// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core.Models;
using Aurora.Music.Core.Storage;
using Aurora.Music.Core.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace Aurora.Music.Services
{
    public sealed class PodcastsFetcher : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            var posts = await SQLOperator.Current().GetPodcastListBriefAsync();
            List<Task> tasks = new List<Task>();
            foreach (var item in posts)
            {
                tasks.Add(Task.Run(async () =>
                {
                    Podcast p = new Podcast(item);
                    if (await p.FindUpdated())
                    {
                        Toast.SendPodcast(p);
                    }
                }));
            }
            await Task.WhenAll(tasks);
            deferral.Complete();
        }
    }
}
