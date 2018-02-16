// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core.Extension;
using Aurora.Music.Core.Models;
using Aurora.Shared.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;

namespace Aurora.Music.ViewModels
{
    class PodcastMarketViewModel : ViewModelBase
    {
        public ObservableCollection<GenericMusicItemViewModel> TopList { get; set; }

        public ObservableCollection<PodcastGroup> Genres { get; set; }



        /// <summary>
        /// <see cref="https://affiliate.itunes.apple.com/resources/documentation/genre-mapping/"/>
        /// </summary>
        private static readonly List<KeyValuePair<string, string>> genres =
            new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Arts", "1301"),
                new KeyValuePair<string, string>("Comedy", "1303"),
                new KeyValuePair<string, string>("Education", "1304"),
                new KeyValuePair<string, string>("Kids & Family", "1305"),
                new KeyValuePair<string, string>("Health", "1307"),
                new KeyValuePair<string, string>("TV & Film", "1309"),
                new KeyValuePair<string, string>("Music", "1310"),
                new KeyValuePair<string, string>("News & Politics", "1311"),
                new KeyValuePair<string, string>("Religion & Spirituality", "1314"),
                new KeyValuePair<string, string>("Science & Medicine", "1315"),
                new KeyValuePair<string, string>("Sports & Recreation", "1316"),
                new KeyValuePair<string, string>("Technology", "1318"),
                new KeyValuePair<string, string>("Business", "1321"),
                new KeyValuePair<string, string>("Games & Hobbies", "1323"),
                new KeyValuePair<string, string>("Society & Culture", "1324"),
                new KeyValuePair<string, string>("Government & Organizations", "1325")
            };

        public PodcastMarketViewModel()
        {
            TopList = new ObservableCollection<GenericMusicItemViewModel>();
            Genres = new ObservableCollection<PodcastGroup>();
        }

        public void Fetch()
        {
            Task.Run(async () =>
            {
                var res = await Podcast.GetiTunesTop(15);

                await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                {
                    TopList.Clear();
                    foreach (var item in res)
                    {
                        TopList.Add(new GenericMusicItemViewModel(item));
                    }

                });
            });
            Task.Run(async () =>
            {
                foreach (var g in genres)
                {
                    await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
                    {
                        var p = new PodcastGroup
                        {
                            Title = g.Key,
                            GenreID = g.Value
                        };
                        Genres.Add(p);
                        await p.GetItems(15);
                    });
                }
            });
        }
    }

    internal class PodcastGroup : ViewModelBase
    {
        public ObservableCollection<GenericMusicItemViewModel> Items { get; set; }

        public string Title { get; set; }
        public string GenreID { get; internal set; }

        public PodcastGroup()
        {
            Items = new ObservableCollection<GenericMusicItemViewModel>();
        }

        internal async Task GetItems(int m)
        {
            var res = await ITunesSearcher.TopGenres(GenreID, m);
            foreach (var a in res.feed.entry)
            {
                Items.Add(new GenericMusicItemViewModel()
                {
                    Title = a.Name.label,
                    Description = a.Summary?.label,
                    Addtional = a.Artist.label,
                    OnlineAlbumID = a.ID.attributes["im:id"],
                    Artwork = new Uri(a.Image[2].label)
                });
            }
        }
    }
}
