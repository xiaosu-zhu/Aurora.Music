// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core;
using Aurora.Music.Core.Storage;
using Aurora.Shared.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.System.Threading;

namespace Aurora.Music.ViewModels
{
    class ArtistsPageViewModel : ViewModelBase
    {
        private ObservableCollection<GroupedItem<ArtistViewModel>> aritstList;

        public ObservableCollection<GroupedItem<ArtistViewModel>> ArtistList
        {
            get { return aritstList; }
            set { SetProperty(ref aritstList, value); }
        }

        public DelegateCommand PlayAll
        {
            get
            {
                return new DelegateCommand(async () =>
                {
                    await MainPageViewModel.Current.InstantPlay(await FileReader.GetAllSongAsync());
                });
            }
        }

        private string artistsCount;
        public string ArtistsCount
        {
            get { return artistsCount; }
            set { SetProperty(ref artistsCount, value); }
        }

        private string songsCount;
        public string SongsCount
        {
            get { return songsCount; }
            set { SetProperty(ref songsCount, value); }
        }

        public ArtistsPageViewModel()
        {
            ArtistList = new ObservableCollection<GroupedItem<ArtistViewModel>>();
        }

        public async Task GetArtists()
        {
            var opr = SQLOperator.Current();
            var artists = await opr.GetArtistsAsync();
            var grouped = GroupedItem<ArtistViewModel>.CreateGroupsByAlpha(artists.ConvertAll(x => new ArtistViewModel
            {
                Name = x.AlbumArtists,
                SongsCount = x.Count
            }));

            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                ArtistList.Clear();

                long sum = 0;
                foreach (var item in grouped)
                {
                    ArtistList.Add(item);
                    sum += item.Sum(x => x.SongsCount);
                }
                ArtistsCount = SmartFormat.Smart.Format(Consts.Localizer.GetString("SmartArtists"), artists.Count);
                SongsCount = SmartFormat.Smart.Format(Consts.Localizer.GetString("SmartSongs"), sum);

                var t = ThreadPool.RunAsync(async x =>
                {
                    foreach (var item in ArtistList)
                    {
                        foreach (var art in item)
                        {
                            var uri = await opr.GetAvatarAsync(art.RawName);
                            if (Uri.TryCreate(uri, UriKind.Absolute, out var u))
                            {
                                await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
                                {
                                    art.Avatar = u;
                                });
                            }
                            else
                            {

                            }
                        }
                    }
                });
            });
        }

        internal void ChangeSort(string p)
        {
            int sum = 0;

            var artists = ArtistList.ToList();

            var list = new List<ArtistViewModel>();

            switch (p)
            {
                case "Name":
                    ArtistList.Clear();
                    foreach (var artist in artists)
                    {
                        list.AddRange(artist);
                    }
                    var grouped = GroupedItem<ArtistViewModel>.CreateGroupsByAlpha(list);
                    foreach (var item in grouped)
                    {
                        ArtistList.Add(new GroupedItem<ArtistViewModel>(item.Key, item));
                        sum += item.Sum(x => x.SongsCount);
                    }
                    ArtistsCount = SmartFormat.Smart.Format(Consts.Localizer.GetString("SmartArtists"), artists.Count);
                    SongsCount = SmartFormat.Smart.Format(Consts.Localizer.GetString("SmartSongs"), sum);
                    break;
                default:
                    break;
            }
        }
    }
}
