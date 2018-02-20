// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Music.Core.Storage;
using Aurora.Shared;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using Aurora.Shared.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;

namespace Aurora.Music.ViewModels
{
    class HomePageViewModel : ViewModelBase
    {
        private PlayerStatus playerStatus;

        public string WelcomeTitle
        {
            get
            {
                var fes = Tools.IsFestival(DateTime.Today);
                switch (fes)
                {
                    case Festival.None:
                        return string.Format(Consts.Localizer.GetString("WelcomeTitleText"), DateTime.Now.GetHourString());
                    case Festival.Valentine:
                        return Consts.Localizer.GetString("ValentineWelcome");
                    case Festival.Halloween:
                        return Consts.Localizer.GetString("HalloweenWelcome");
                    case Festival.Xmas:
                        return Consts.Localizer.GetString("ChristmasWelcome");
                    case Festival.Fool:
                        return string.Format(Consts.Localizer.GetString("WelcomeTitleText"), DateTime.Now.GetHourString());
                    default:
                        return string.Format(Consts.Localizer.GetString("WelcomeTitleText"), DateTime.Now.GetHourString());
                }
            }
        }

        public DelegateCommand PlayRandom
        {
            get => new DelegateCommand(async () =>
            {
                var list = new List<Song>();
                foreach (var item in RandomList)
                {
                    list.AddRange(await item.GetSongsAsync());
                }
                await MainPageViewModel.Current.InstantPlay(list);
            });
        }

        public DelegateCommand PlayFav
        {
            get => new DelegateCommand(async () =>
            {
                var list = new List<Song>();
                foreach (var item in FavList)
                {
                    list.AddRange(await item.GetSongsAsync());
                }
                await MainPageViewModel.Current.InstantPlay(list);
            });
        }

        public DelegateCommand ReRandom
        {
            get => new DelegateCommand(async () =>
            {
                var ran = await FileReader.GetRandomListAsync();
                RandomList.Clear();
                foreach (var item in ran)
                {
                    RandomList.Add(new GenericMusicItemViewModel(item));
                }
            });
        }

        public HomePageViewModel()
        {
            HeroList.Add(new GenericMusicItemViewModel());
            HeroList.Add(new GenericMusicItemViewModel());
            HeroList.Add(new GenericMusicItemViewModel());
            HeroList.Add(new GenericMusicItemViewModel());
            FavList.Add(new GenericMusicItemViewModel());
            FavList.Add(new GenericMusicItemViewModel());
            FavList.Add(new GenericMusicItemViewModel());
            FavList.Add(new GenericMusicItemViewModel());
            FavList.Add(new GenericMusicItemViewModel());
            FavList.Add(new GenericMusicItemViewModel());
            FavList.Add(new GenericMusicItemViewModel());
            FavList.Add(new GenericMusicItemViewModel());
            FavList.Add(new GenericMusicItemViewModel());
            FavList.Add(new GenericMusicItemViewModel());
            FavList.Add(new GenericMusicItemViewModel());
            FavList.Add(new GenericMusicItemViewModel());
            RandomList.Add(new GenericMusicItemViewModel());
            RandomList.Add(new GenericMusicItemViewModel());
            RandomList.Add(new GenericMusicItemViewModel());
            RandomList.Add(new GenericMusicItemViewModel());
            RandomList.Add(new GenericMusicItemViewModel());
            RandomList.Add(new GenericMusicItemViewModel());
            RandomList.Add(new GenericMusicItemViewModel());
            RandomList.Add(new GenericMusicItemViewModel());
            RandomList.Add(new GenericMusicItemViewModel());
            RandomList.Add(new GenericMusicItemViewModel());
            RandomList.Add(new GenericMusicItemViewModel());
            RandomList.Add(new GenericMusicItemViewModel());

            Task.Run(async () =>
            {
                await Load();
            });
        }

        public ObservableCollection<GenericMusicItemViewModel> FavList { get; set; } = new ObservableCollection<GenericMusicItemViewModel>();
        public ObservableCollection<GenericMusicItemViewModel> RandomList { get; set; } = new ObservableCollection<GenericMusicItemViewModel>();

        public ObservableCollection<GenericMusicItemViewModel> HeroList { get; set; } = new ObservableCollection<GenericMusicItemViewModel>();

        public async Task Load()
        {
            var hero = await FileReader.GetHeroListAsync();

            playerStatus = await PlayerStatus.LoadAsync();

            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
            {
                HeroList.Clear();
                foreach (var item in hero)
                {
                    if (item.IsNullorEmpty())
                        continue;
                    var pic = (from i in item where !i.PicturePath.IsNullorEmpty() select i.PicturePath).FirstOrDefault();
                    HeroList.Add(new GenericMusicItemViewModel()
                    {
                        IDs = item.Select(x => x.IDs).Aggregate((a, b) =>
                        {
                            return a.Concat(b).ToArray();
                        }),
                        Title = item.Key,
                        Artwork = pic.IsNullorEmpty() ? null : new Uri(pic),
                        MainColor = pic.IsNullorEmpty() ? Palette.Gray : await ImagingHelper.GetMainColor(pic.IsNullorEmpty() ? null : new Uri(pic)),
                        InnerType = MediaType.PlayList
                    });
                }

                if (playerStatus != null && playerStatus.Songs != null)
                {
                    var pic = (from i in playerStatus.Songs where !i.PicturePath.IsNullorEmpty() select i.PicturePath).FirstOrDefault();
                    HeroList.Add(new GenericMusicItemViewModel()
                    {
                        IDs = null,
                        Title = Consts.Localizer.GetString("PlayingHistoryText"),
                        Artwork = pic.IsNullorEmpty() ? null : new Uri(pic),
                        MainColor = pic.IsNullorEmpty() ? Palette.Gray : await ImagingHelper.GetMainColor(pic.IsNullorEmpty() ? null : new Uri(pic)),
                        InnerType = MediaType.PlayList
                    });
                }
            });

            var ran = await FileReader.GetRandomListAsync();
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                RandomList.Clear();
                foreach (var item in ran)
                {
                    RandomList.Add(new GenericMusicItemViewModel(item));
                }
            });

            var fav = await FileReader.GetFavListAsync();
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                FavList.Clear();
                foreach (var item in fav)
                {
                    FavList.Add(new GenericMusicItemViewModel(item));
                }
            });

        }

        internal async Task RestorePlayerStatus()
        {
            await MainPageViewModel.Current.InstantPlay(playerStatus.Songs, playerStatus.Index);
            PlaybackEngine.PlaybackEngine.Current.Seek(playerStatus.Position);
        }
    }
}
