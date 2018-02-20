// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Shared.Extensions;
using Aurora.Shared.MVVM;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;

namespace Aurora.Music.ViewModels
{
    class PodcastPageViewModel : ViewModelBase
    {

        private ObservableCollection<SongViewModel> songsList;
        public ObservableCollection<SongViewModel> SongsList
        {
            get { return songsList; }
            set { SetProperty(ref songsList, value); }
        }

        private Uri heroImage;
        public Uri HeroImage
        {
            get { return heroImage; }
            set { SetProperty(ref heroImage, value); }
        }

        private string desc;
        public string Description
        {
            get { return desc; }
            set { SetProperty(ref desc, value); }
        }

        private string lastUpdate;
        public string LastUpdate
        {
            get { return lastUpdate; }
            set { SetProperty(ref lastUpdate, value); }
        }

        private string title;
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        public PodcastPageViewModel()
        {
            SongsList = new ObservableCollection<SongViewModel>();
        }

        public DelegateCommand PlayAll
        {
            get
            {
                return new DelegateCommand(async () =>
                {
                    await MainPageViewModel.Current.InstantPlay(Model);
                });
            }
        }

        private bool isSubscribe;
        public bool IsSubscribe
        {
            get { return isSubscribe; }
            set { SetProperty(ref isSubscribe, value); }
        }

        public DelegateCommand ToggleSubscribe
        {
            get
            {
                return new DelegateCommand(async () =>
                {
                    Model.Subscribed = !Model.Subscribed;
                    await Model.SaveAsync();
                    IsSubscribe = Model.Subscribed;
                    MainPage.Current.PopMessage(Model.Subscribed ? "Subscribed" : "Un-Subscribed");
                });
            }
        }

        public DelegateCommand Refresh
        {
            get
            {
                return new DelegateCommand(async () =>
                {
                    await Model.Refresh();
                    SongsList.Clear();
                    uint i = 0;
                    foreach (var item in Model)
                    {
                        SongsList.Add(new SongViewModel(item)
                        {
                            Index = ++i
                        });
                    }
                    LastUpdate = Model.LastUpdate.PubDatetoString($"'{Consts.Today}'", "ddd", "M/dd ddd", "yy/M/dd", Consts.Next, Consts.Last);
                    Description = Model.Description;
                    Title = Model.Title;
                    HeroImage = new Uri(Model.HeroArtworks[0]);
                    IsSubscribe = Model.Subscribed;
                    MainPage.Current.PopMessage("Refreshed");
                });
            }
        }

        public Podcast Model { get; private set; }

        public async Task Init(int ID)
        {
            Model = await Podcast.ReadFromLocalAsync(ID);
            if (Model == null)
            {
                return;
            }

            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                SongsList.Clear();
                uint i = 0;
                foreach (var item in Model)
                {
                    SongsList.Add(new SongViewModel(item)
                    {
                        Index = ++i
                    });
                }
                LastUpdate = Model.LastUpdate.PubDatetoString($"'{Consts.Today}'", "ddd", "M/dd ddd", "yy/M/dd", Consts.Next, Consts.Last);
                Description = Model.Description;
                IsSubscribe = Model.Subscribed;
                Title = Model.Title;
                HeroImage = new Uri(Model.HeroArtworks[0]);
            });
        }

        internal async Task PlayAt(SongViewModel songViewModel)
        {
            var i = (int)songViewModel.Index - 1;
            if (Model.Count < i + 20)
            {
                var s = Model[i];
                var k = Model.Count < 20 ? Model.ToList() : Model.GetRange(Model.Count - 20, 20);
                await MainPageViewModel.Current.InstantPlay(k, k.IndexOf(s));
            }
            else
            {
                await MainPageViewModel.Current.InstantPlay(Model.GetRange(i, 20), 0);
            }
        }
    }
}
