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
using Windows.UI.Xaml;

namespace Aurora.Music.ViewModels
{
    class HomePageViewModel : ViewModelBase
    {
        private PlayerStatus playerStatus;

        private string welcomeTitle;
        public string WelcomeTitle
        {
            get { return welcomeTitle; }
            set { SetProperty(ref welcomeTitle, value); }
        }


        public string GetWelcomeTitle()
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
            WelcomeTitle = GetWelcomeTitle();
            HeroList.Add(new HeroItemViewModel());
            HeroList.Add(new HeroItemViewModel());
            HeroList.Add(new HeroItemViewModel());
            HeroList.Add(new HeroItemViewModel());
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

            Application.Current.LeavingBackground += Current_LeavingBackground;

            Task.Run(async () =>
            {
                await Load();
            });
        }

        private async void Current_LeavingBackground(object sender, Windows.ApplicationModel.LeavingBackgroundEventArgs e)
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                WelcomeTitle = GetWelcomeTitle();
            });
        }

        public ObservableCollection<GenericMusicItemViewModel> FavList { get; set; } = new ObservableCollection<GenericMusicItemViewModel>();
        public ObservableCollection<GenericMusicItemViewModel> RandomList { get; set; } = new ObservableCollection<GenericMusicItemViewModel>();

        public ObservableCollection<HeroItemViewModel> HeroList { get; set; } = new ObservableCollection<HeroItemViewModel>();

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
                    var pic = (from i in item where !i.PicturePath.IsNullorEmpty() select i.PicturePath).Distinct().ToList();
                    pic.Shuffle();
                    HeroList.Add(new HeroItemViewModel()
                    {
                        IDs = item.Select(x => x.IDs).Aggregate((a, b) =>
                        {
                            return a.Concat(b).ToArray();
                        }),
                        Title = item.Key,
                        Artwork = pic.Count < 1 ? null : new Uri(pic[0]),
                        Artwork1 = pic.Count < 2 ? null : new Uri(pic[1]),
                        Artwork2 = pic.Count < 3 ? null : new Uri(pic[2]),
                        Artwork3 = pic.Count < 4 ? null : new Uri(pic[3]),
                        Artwork4 = pic.Count < 5 ? null : new Uri(pic[4]),
                        Artwork5 = pic.Count < 6 ? null : new Uri(pic[5]),
                        Artwork6 = pic.Count < 7 ? null : new Uri(pic[6]),
                        Artwork7 = pic.Count < 8 ? null : new Uri(pic[7]),
                        MainColor = pic.Count < 1 ? Palette.Gray : await ImagingHelper.GetMainColor(pic.IsNullorEmpty() ? null : new Uri(pic[0])),
                        InnerType = MediaType.PlayList
                    });
                }

                if (playerStatus != null && playerStatus.Songs != null)
                {
                    var pic = (from i in playerStatus.Songs where !i.PicturePath.IsNullorEmpty() select i.PicturePath).Distinct().ToList();
                    pic.Shuffle();
                    HeroList.Add(new HeroItemViewModel()
                    {
                        IDs = null,
                        Title = Consts.Localizer.GetString("PlayingHistoryText"),
                        Artwork = pic.Count < 1 ? null : new Uri(pic[0]),
                        Artwork1 = pic.Count < 2 ? null : new Uri(pic[1]),
                        Artwork2 = pic.Count < 3 ? null : new Uri(pic[2]),
                        Artwork3 = pic.Count < 4 ? null : new Uri(pic[3]),
                        Artwork4 = pic.Count < 5 ? null : new Uri(pic[4]),
                        Artwork5 = pic.Count < 6 ? null : new Uri(pic[5]),
                        Artwork6 = pic.Count < 7 ? null : new Uri(pic[6]),
                        Artwork7 = pic.Count < 8 ? null : new Uri(pic[7]),
                        MainColor = pic.Count < 1 ? Palette.Gray : await ImagingHelper.GetMainColor(pic.IsNullorEmpty() ? null : new Uri(pic[0])),
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

        internal void Unload()
        {
            Application.Current.LeavingBackground -= Current_LeavingBackground;
        }

        internal async Task RestorePlayerStatus()
        {
            await MainPageViewModel.Current.InstantPlay(playerStatus.Songs, playerStatus.Index);
            PlaybackEngine.PlaybackEngine.Current.Seek(playerStatus.Position);
        }
    }
}
